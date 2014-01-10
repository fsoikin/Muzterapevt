using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using erecruit.Composition;

namespace Mut.Data
{
	public class EmailServiceConfig
	{
		public int Id { get; set; }
		public int CheckPeriodMilliseconds { get; set; }
		public int LastProcessedEmailId { get; set; }
		public int BatchSize { get; set; }
		public string DefaultFromEmail { get; set; }
		public string DefaultFromName { get; set; }

		public EmailServiceConfig() {
			this.CheckPeriodMilliseconds = 10000;
			this.BatchSize = 20;
		}

		[Export]
		class Mapping : IModelMapping
		{
			public void Map( DbModelBuilder b ) {
				b.Entity<EmailServiceConfig>().ToTable( "EmailServiceConfig" );
			}
		}
	}
}