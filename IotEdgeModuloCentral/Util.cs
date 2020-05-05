using IotEdgeModuloCentral.Tipos;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;

namespace IotEdgeModuloCentral
{
    public static class Util
    {
        public static void Log(string msg)
        {
            //if (1==0)
            //{
            //    Console.WriteLine(msg);
            //}
        }

        public static void LogFixo(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void LogErro(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void GravarDados(MessageBodyIoTCentral messageBodyIoTCentral)
        {
            MetodoNaoImplementado("GravarDados");
        }

        public static void MetodoNaoImplementado(string nomeMetodo, string valorGerado = "")
        {
            if (String.IsNullOrEmpty(valorGerado))
            {
                LogErro($"Erro - Metodo nao implementado: {nomeMetodo}()");
            }
            else
            {
                LogErro($"Erro - Metodo nao implementado: {nomeMetodo}() - valor gerado: {valorGerado}");
            }
        }
    }
}
