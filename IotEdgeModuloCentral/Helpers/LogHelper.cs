using System;

namespace IotEdgeModuloCentral.Helpers
{
    public class LogHelper
    {
        public LogHelper()
        {
            Log($"[LogHelper] Iniciando helper...");
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public void Log(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(ex.ToString());
            Console.WriteLine($"[LogHelper.Log] {ex}");
        }

        public void Log(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(msg);
            Console.WriteLine($"[LogHelper.Log] {msg}");
        }

        public void Warning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(msg);
            Console.WriteLine($"[LogHelper.Warning] {msg}");
        }

        public void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
            Console.WriteLine($"[LogHelper.Info] {msg}");
        }

        public void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.WriteLine($"[LogHelper.Error] {msg}");
        }

        public void Error(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[LogHelper.Error] {ex}");
        }

        public void MetodoNaoImplementado(string nomeMetodo, string valorGerado = "")
        {
            if (String.IsNullOrEmpty(valorGerado))
            {
                Error($"Erro - Metodo nao implementado: {nomeMetodo}()");
            }
            else
            {
                Error($"Erro - Metodo nao implementado: {nomeMetodo}() - valor gerado: {valorGerado}");
            }
        }

    }

}
