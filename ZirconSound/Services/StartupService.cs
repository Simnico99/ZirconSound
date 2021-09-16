using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.Services
{
    class StartupService
    {

        public static async Task<Process> StartLavalinkAsync()
        {
            var path = Directory.GetCurrentDirectory();

            string batDir = @$"{path}\Lavalink";
            var proc = new Process();
            proc.StartInfo.WorkingDirectory = batDir;
            proc.StartInfo.FileName = $@"{batDir}\StartLavalink.bat";
            proc.StartInfo.CreateNoWindow = false;
            await Task.Run(() => proc.Start());

            return proc;
        }
    }
}
