using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	[Export, TransactionScoped]
	class Database : DbContext
	{
		[Import] public IEnumerable<IModelMapping> Mappings { get; set; }

		public Database( IDatabaseConfig config ) : base( config.ConnectionString ) { }

		protected override void OnModelCreating( DbModelBuilder modelBuilder )
		{
			foreach ( var m in Mappings ) m.Map( modelBuilder );
		}

		[Export, TransactionScoped]
		class Reprository<T> : IRepository<T> where T : class
		{
			[Import] public Database Db { get; set; }

			public T Find( object key ) { return Db.Set<T>().Find( key ); }
			public T Add( T t ) { return Db.Set<T>().Add( t ); }
			public void Remove( T t ) { Db.Set<T>().Remove( t ); }
			public IQueryable<T> All { get { return Db.Set<T>(); } }
		}

		[Export, TransactionScoped]
		class UnitOfWork : IUnitOfWork
		{
			[Import] public Database Db { get; set; }

			public event EventHandler BeforeCommit;
			public event EventHandler AfterCommit;

			public void Commit()
			{
				var h = BeforeCommit; if ( h != null ) h( this, EventArgs.Empty );
				Db.SaveChanges();
				h = AfterCommit; if ( h != null ) h( this, EventArgs.Empty );
			}
		}
	}

	public interface IRepository<T> where T : class
	{
		T Find( object key );
		T Add( T t );
		void Remove( T t );
		IQueryable<T> All { get; }
	}

	public interface IUnitOfWork
	{
		event EventHandler BeforeCommit;
		event EventHandler AfterCommit;
		void Commit();
	}

	public interface IModelMapping
	{
		void Map( DbModelBuilder mb );
	}

	public interface IDatabaseConfig
	{
		string ConnectionString { get; }
	}
}