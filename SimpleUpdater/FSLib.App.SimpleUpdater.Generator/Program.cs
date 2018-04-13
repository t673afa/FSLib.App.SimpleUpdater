using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FSLib.App.SimpleUpdater.Generator.Defination;

namespace FSLib.App.SimpleUpdater.Generator
{
	using System.IO;
	using System.Threading;

	using BuilderInterface;

	static class Program
	{
		internal static bool Running;
		[DllImport("kernel32.dll")]
		static extern bool FreeConsole();
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Running = true;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var startArg = new StratArg();

			if (args.Length > 0)
			{
				 startArg = GetStratArg();

				if (string.IsNullOrEmpty(startArg.File) || !File.Exists(startArg.File))
				{
					Console.WriteLine(SR.Program_Main_NoProjectFileSpecified);
					return;
				}
			}
			else
			{
				FreeConsole();
			}

			if (!startArg.Build)
			{
				Application.Run(new Main() { PreloadFile = startArg.File });
			}
			else
			{
				if (startArg.NoUI)
				{
					RunBuild(startArg.File, false,startArg);
				}
				else if (startArg.MiniUI)
				{
					FreeConsole();
					RunBuild(startArg.File, true, startArg);
				}
				else
				{
					FreeConsole();
					Application.Run(new Main() { PreloadFile = startArg.File, AutoBuild = true });
				}
			}

			/*
			var file = string.Empty;
			var build = false;
			var noui = false;
			var miniui = false;

			if (args.Length > 0)
			{
				miniui = args.Any(s => string.Compare(s, "/miniui", true) == 0);
				noui = args.Any(s => string.Compare(s, "/noui", true) == 0);
				build = args.Any(s => string.Compare(s, "/build", true) == 0);
				file = args.Last();

				try
				{
					if (!System.IO.File.Exists(file))
						file = null;
				}
				catch (Exception ex)
				{
					file = null;
				}
			}
			else
			{
				FreeConsole();
			}

			if (build)
			{
				if (string.IsNullOrEmpty(file) || !File.Exists(file))
				{
					Console.WriteLine(SR.Program_Main_NoProjectFileSpecified);
					return;
				}

				if (noui)
				{
					RunBuild(file, false);
				}
				else if (miniui)
				{
					FreeConsole();
					RunBuild(file, true);
				}
				else
				{
					FreeConsole();
					Application.Run(new Main() { PreloadFile = file, AutoBuild = true });
				}
			}
			else
			{
				Application.Run(new Main() { PreloadFile = file });
			}*/
		}

		static void RunBuild(string file, bool miniui)
		{
			var builder = miniui ? (BuilderInterfaceBase)new FormBuildInterface() : new ConsoleBuildInterface();
			var running = true;
			builder.WorkerShutdown += (s, e) => running = false;
			builder.Build(file);

			while (running)
			{
				Thread.Sleep(50);
			}
		}
		
		static void RunBuild(string file, bool miniui,StratArg stratArg)
		{
			var builder = miniui ? (BuilderInterfaceBase)new FormBuildInterface() : new ConsoleBuildInterface();
			var running = true;
			builder.WorkerShutdown += (s, e) => running = false;

			var project = AuProject.LoadFile(file);

			if (!string.IsNullOrEmpty(stratArg.Version))
			{
				project.UpdateInfo.AppVersion = stratArg.Version;
			}
			if (!string.IsNullOrEmpty(stratArg.AppName))
			{
				project.UpdateInfo.AppName = stratArg.AppName;
			}
			if (!string.IsNullOrEmpty(stratArg.OutDir))
			{
				if (!Directory.Exists(stratArg.OutDir))
				{
					Directory.CreateDirectory(stratArg.OutDir);
				}
				project.DestinationDirectory = stratArg.OutDir;
			}
			if (!string.IsNullOrEmpty(stratArg.SourceDir))
			{
				project.ApplicationDirectory = stratArg.SourceDir;
			}

			builder.Build(project);

			while (running)
			{
				Thread.Sleep(50);
			}
		}


		/// <summary>
		/// 初始化工作参数
		/// </summary>
		private static StratArg GetStratArg()
		{
			var stratAppInfo = new StratArg();	
			var args = Environment.GetCommandLineArgs();
			var index = 0;
			while (index < args.Length)
			{
				var name = args[index++];
				switch (name)
				{
					case "/miniui":
						stratAppInfo.MiniUI =true;
						break;
					case "/noui":
						stratAppInfo.NoUI = true;
						break;
					case "/build":
						stratAppInfo.Build = true;
						break;
					case "/file":
						stratAppInfo.File = args[index++];
						break;
					case "/appname":
						stratAppInfo.AppName = args[index++];
						break;
					case "/version":
						stratAppInfo.Version = args[index++];
						break;
					case "/sourcedir":
						stratAppInfo.SourceDir = args[index++];
						break;
					case "/outdir":
						stratAppInfo.OutDir = args[index++];
						break;
				}
			}
			return stratAppInfo;

		}
	}
	/// <summary>
	/// 启动参数
	/// </summary>
	public class StratArg
	{
		/// <summary>
		/// 最小化UI
		/// </summary>
		public bool MiniUI { get; set; }

		/// <summary>
		/// 控制台模式
		/// </summary>
		public bool NoUI { get; set; }

		/// <summary>
		/// 是否构建
		/// </summary>
		public bool Build { get; set; }

		/// <summary>
		/// 版本发布配置信息文件路径
		/// </summary>
		public string File { get; set; }

		/// <summary>
		/// 程序名称
		/// </summary>
		public string AppName { get; set; }

		/// <summary>
		/// 版本号
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// 源目录
		/// </summary>
		public string SourceDir { get; set; }

		/// <summary>
		/// 输出目录
		/// </summary>
		public string OutDir { get; set; }
	}
}
