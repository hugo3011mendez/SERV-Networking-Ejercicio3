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
        static readonly object l = new object(); // Creo el objeto del lock, porque los hilos están usando recursos iguales

        static List<Cliente> clientes = new List<Cliente>(); // Creo una colección de clientes para ir guardando en ella los clientes que se conectan
        static List<int> numeros = new List<int>(); // Creo una colección que guarde los números que le han tocado a los clientes

        static void funcionCliente(object cl)
        {
            Cliente cliente = (Cliente)cl;

            Random generador = new Random();
            numeros.Add(generador.Next(1,21));

            using (NetworkStream ns = new NetworkStream(cliente.SClient))
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

            s.Listen(6); // Se queda esperando una conexión y se establece la cola a 6 como máximo

            while (true)
            {
                Socket sClient = s.Accept(); // Aceptamos la conexión del cliente
                // Después de aceptar la conexión, añadimos a la colección un nuevo cliente pasándole su Socket como parámetro para así inicializarlo
                clientes.Add(new Cliente(sClient));

                // Uso la funcionCliente para el hiloCliente y lo lanzo pasándole el último cliente añadido a la colección como parámetro
                Thread hiloCliente = new Thread(funcionCliente);
                hiloCliente.Start(clientes[clientes.Count - 1]);
            }
        }
    }
}
