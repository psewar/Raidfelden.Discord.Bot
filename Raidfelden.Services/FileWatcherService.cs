using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Raidfelden.Services
{
	public interface IFileWatcherService
	{
		Task Add(string path, NotifyFilters notifyFilters, Action<string> action);
	}

	public class FileWatcherService : IFileWatcherService
	{
		private static readonly ConcurrentDictionary<FileSystemWatcher, List<Action<string>>> FileSystemWatcherCache;

		static FileWatcherService()
		{
			FileSystemWatcherCache = new ConcurrentDictionary<FileSystemWatcher, List<Action<string>>>();
		}

		public async Task Add(string path, NotifyFilters notifyFilters, Action<string> action)
		{
			var watcher = new FileSystemWatcher(path) {NotifyFilter = notifyFilters};

			// Add event handlers.
			watcher.Changed += OnChanged;
			watcher.Created += OnChanged;
			watcher.Deleted += OnChanged;
			//watcher.Renamed += OnRenamed;

			FileSystemWatcherCache.AddOrUpdate(watcher, new List<Action<string>>() {action}, (systemWatcher, list) =>
			{
				list.Add(action);
				return list;
			});

			await Task.CompletedTask;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			var watcher = (FileSystemWatcher)source;
			
			if (!FileSystemWatcherCache.TryGetValue(watcher, out List<Action<string>> actions)) return;
			foreach (var action in actions)
			{
				action(e.FullPath);
			}
		}

		//private void OnRenamed(object source, RenamedEventArgs e)
		//{
		//	// Specify what is done when a file is renamed.
		//	Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
		//}
	}
}
