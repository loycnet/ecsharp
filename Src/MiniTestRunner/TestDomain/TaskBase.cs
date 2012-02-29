﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Loyc.Collections;
using System.Security;

namespace MiniTestRunner.TestDomain
{
	abstract class TaskBase : PropertyChangedHelper, ITaskEx
	{
		TestStatus _status;
		DateTime _lastRunAt;
		TimeSpan _runTime;
		string _summary;
		IList<IRowModel> _children = EmptyList<IRowModel>.Value;

		/* Apparently we can't control remoting lifetime in partial-trust!??!?!
		 * This method causes a TypeLoadException.
		[SecurityCritical]
		public override object InitializeLifetimeService()
		{
			return null; // This object will exist as long as its AppDomain does
		}*/

		public TestStatus Status
		{ 
			get { return _status; }
			set { Set(ref _status, value, "Status"); }
		}
		public DateTime LastRunAt
		{ 
			get { return _lastRunAt; }
			set { Set(ref _lastRunAt, value, "LastRunAt"); }
		}
		public TimeSpan RunTime
		{ 
			get { return _runTime; }
			set { Set(ref _runTime, value, "RunTime"); }
		}

		public virtual int Priority
		{
			get { return 0; }
		}
		public virtual int MaxThreads
		{
			get { return int.MaxValue; }
		}
		public virtual bool IsCompleted
		{
			get { return Status != TestStatus.NotRun && Status != TestStatus.Running; }
		}

		public virtual IEnumerable<ITask> RunOnCurrentThread()
		{
			try {
				Status = TestStatus.Running;
				LastRunAt = DateTime.Now;
				var timer = new Stopwatch();
				timer.Start();
				RunCore();
				RunTime = timer.Elapsed;
			} finally {
				Status = TestStatus.Inconclusive;
			}
			Status = TestStatus.Success;
			return null;
		}
		
		protected abstract void RunCore();

		public Stream OutputStream { get; set; }

		protected internal static T GetPropertyValue<T>(object obj, string propertyName, T failValue)
		{
			PropertyInfo propertyInfo;
			if (obj != null && (propertyInfo = obj.GetType().GetProperty(propertyName)) != null)
			{
				object result = propertyInfo.GetGetMethod().Invoke(obj, null);
				if (result is T)
					return (T)result;
			}
			return failValue;
		}

		public virtual IList<IRowModel> Children
		{
			get { return _children; }
			set { Set(ref _children, value ?? EmptyList<IRowModel>.Value, "Children"); }
		}

		public virtual string Summary
		{
			get { return _summary; }
			set { Set(ref _summary, value, "Summary"); }
		}

		public virtual void Abort(System.Threading.Thread thread)
		{
			throw new NotImplementedException();
		}
		public virtual IEnumerable<ITask> Prerequisites(IEnumerable<ITask> concurrentTasks)
		{
			return null;
		}
	}
}