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
	public interface ISiteService
	{
		Guid CurrentSiteId { get; }
		string CurrentSiteTheme { get; }
	}
}