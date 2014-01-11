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
	public class EmailService : IDisposable
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

			_running = onNeedToCheck.ObserveOn( ThreadPoolScheduler.Instance )
				.SelectMany( _1 => Observable.Using( () => tran.OpenTransaction(), tranScope =>
					from _4 in Observable.Return( 0 )
					let cfg = GetConfig( config() )
					let batch = _emails().All.Where( e => e.Id > cfg.LastProcessedEmailId ).OrderBy( x => x.Id ).Take( cfg.BatchSize )

					from e in batch
						.ToList()
						.ToObservable()
						.Do( e => log.DebugFormat( "Sending e-mail '{0}' to '{1}' (record id={2}).", e.Subject, e.ToEmail, e.Id ) )

					from sent in Send( e, cfg )
						.ToObservable()
						.Do( _5 => {
							using ( tranScope.Transaction.OpenScope() ) { // This is needed, because Send may return on a different thread
								cfg.LastProcessedEmailId = e.Id;
								_unitOfwork().Commit();
							}
						} )

					select Unit.Default ) )
				.Catch( ( Exception ex ) => {
					log.Error( ex );
					return Observable.Timer( TimeSpan.FromSeconds( 1 ) ).Select( __ => Unit.Default );
				} )
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

				var from =
					 !e.FromEmail.NullOrEmpty() ? new MailAddress( e.FromEmail, e.FromName ) :
					 !config.DefaultFromEmail.NullOrEmpty() ? new MailAddress( config.DefaultFromEmail, config.DefaultFromName )
					 : null;
				if ( from != null ) msg.From = from;

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
		}
	}
}