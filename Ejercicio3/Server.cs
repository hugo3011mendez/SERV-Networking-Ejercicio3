using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace Ejercicio3
{
    class Server
    {
        List<int> numeros = new List<int>(); // Creo una colección que guarde los números que le han tocado a los clientes

        static void funcionCliente(object cliente)
        {
            Socket sClient = (Socket)cliente;
            IPEndPoint ieCliente = (IPEndPoint)sClient.RemoteEndPoint;

            using (NetworkStream ns = new NetworkStream(sClient))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {

            }

        }


        static void Main(string[] args)
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Loopback, 31416); // Creo y defino el IPEndPoint del server
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Creo y defino el socket

            try
            {
                s.Bind(ie); // Enlazo el socket al IPEndPoint
            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                // Si está ocupado, lo cambio a otro secundario
                ie.Port = 31415;
                try
                {
                    s.Bind(ie);
                    Console.WriteLine("Servidor lanzado en el puerto " + ie.Port);
                }
                catch (SocketException e1) when (e1.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                {
                    // Si el secundario también está ocupado, cierro el server
                    Console.WriteLine("Puertos ocupados, no se puede lanzar el servidor");
                    s.Close();
                    return;
                }
            }

            s.Listen(6); // Se queda esperando una conexión y se establece la cola a 30

            while (true)
            {
                Socket sClient = s.Accept(); // Aceptamos la conexión del cliente
                Thread hiloCliente = new Thread(funcionCliente);
                hiloCliente.Start(sClient);
            }
        }
    }
}
