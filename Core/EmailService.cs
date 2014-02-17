using System;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using erecruit.Composition;
using erecruit.Utils;
using log4net;
using Mut.Data;

namespace Mut
{
	[Export]
	public sealed class EmailService : IDisposable
	{
		public void SendPendingEmailsNow() {
			_kick.OnNext( Unit.Default );
		}

		public void SendEmail( Email e ) {
			using ( _tran.OpenTransaction() ) {
				_emails().Add( e );
				_unitOfwork().Commit();
			}
			SendPendingEmailsNow();
		}

		readonly Func<IRepository<Email>> _emails;
		readonly Func<IUnitOfWork> _unitOfwork;
		readonly ITransactionService _tran;

		public EmailService( ITransactionService tran, Func<IUnitOfWork> unitOfwork, Func<IRepository<Email>> emails,
			Func<IRepository<EmailServiceConfig>> config, ILog log ) {

			this._emails = emails;
			this._tran = tran;
			this._unitOfwork = unitOfwork;

			var checkPeriod = TimeSpan.FromMilliseconds( FetchConfig( config ).CheckPeriodMilliseconds );
			var onNeedToCheck = _kick.Merge( Observable.Interval( checkPeriod ).Select( _ => Unit.Default ) );
			var updateDb = new Func<ITransactionScope, Func<bool>, IObservable<bool>>( 
				( tranScope, update ) =>
				Observable.Using( 
					() => tranScope.Transaction.OpenScope(),
					_ => Observable
						.Return( Unit.Default )
						.Select( __ => {
							var res = update();
							_unitOfwork().Commit();
							return res;
						} ) ) );

			_running = onNeedToCheck.ObserveOn( ThreadPoolScheduler.Instance )
				.SelectMany( _1 => Observable.Using( () => tran.OpenTransaction(), tranScope =>
					from _4 in Observable.Return( 0 )
					let cfg = GetConfig( config() )
					let batch = _emails().All.Where( e => e.Id > cfg.LastProcessedEmailId && e.Sent == null ).OrderBy( x => x.Id ).Take( cfg.BatchSize ).ToList()
					where batch.Any()

					let sending =
						batch
						.ToObservable()
						.Do( e => log.DebugFormat( "Sending e-mail '{0}' to '{1}' (record id={2}).", e.Subject, e.ToEmail, e.Id ) )
						.SelectMany( e =>
							Send( e, cfg )
							.ToObservable()
							.SelectMany( _ => updateDb( tranScope, () => { e.Sent = DateTime.Now; return true; } ) )
							.Catch( ( Exception ex ) =>
								updateDb( tranScope, () => {
									log.Error( ex );
									e.ErrorCount++;
									e.LastError = ex.ToString();
									if ( e.ErrorCount >= cfg.MaxErrorsPerEmail ) {
										log.WarnFormat( "Email message '{0}' to '{1}' (record id={2}) has reached the maximum number of errors {3} and will not be reattempted again.", e.Subject, e.ToEmail, e.Id, cfg.MaxErrorsPerEmail );
										return true;
									}
									return false;
								} ) )
							)

					from allSucceeded in sending.Aggregate( true, ( previousSucceeded, thisSucceeded ) => previousSucceeded && thisSucceeded )

					where allSucceeded
					from rememberLastEmailId in updateDb( tranScope, () => { cfg.LastProcessedEmailId = batch.Last().Id; return true; } )

					select Unit.Default ) )
				.Catch( ( Exception ex ) => {
					log.Error( ex );
					return Observable.Timer( TimeSpan.FromSeconds( 1 ) ).Select( __ => Unit.Default );
				} )
				.Do( _ => log.Error( "Restarting email service for some reason." ) )
				.Repeat()
				.Subscribe();
		}

		private async Task Send( Email e, EmailServiceConfig config ) {
			using( var smtp = new SmtpClient() ) {
				var msg = new MailMessage {
					Subject = e.Subject,
					Body = e.Body,
					IsBodyHtml = true,
					To = { new MailAddress( e.ToEmail, e.ToName ) }
				};

				var from = (
					from fromEmail in e.FromEmail.MaybeDefined()
					where !fromEmail.NullOrEmpty()
					from a in new MailAddress( fromEmail, e.FromName )
					select a
					).Or( () =>
					from fromEmail in config.DefaultFromEmail.MaybeDefined()
					where !fromEmail.NullOrEmpty()
					from a in new MailAddress( fromEmail, config.DefaultFromName )
					select a
					);
				if ( from.Kind == Maybe.Kind.Value ) msg.From = from.Value;

				await smtp.SendMailAsync( msg );
			}
		}

		private EmailServiceConfig GetConfig( IRepository<EmailServiceConfig> config ) {
			return config.All.FirstOrDefault() ?? config.Add( new EmailServiceConfig() );
		}

		private EmailServiceConfig FetchConfig( Func<IRepository<EmailServiceConfig>> config ) {
			using ( _tran.OpenTransaction() ) {
				var res = GetConfig( config() );
				_unitOfwork().Commit();
				return res;
			}
		}

		private readonly Subject<Unit> _kick = new Subject<Unit>();
		private IDisposable _running;

		public void Dispose() {
			var a = _running;
			_running = null;
			if ( a != null ) a.Dispose();

			_kick.Dispose();
		}
	}
}