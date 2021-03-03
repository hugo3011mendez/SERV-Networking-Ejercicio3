using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ejercicio3
{
    class Cliente
    {
        static readonly internal object m = new object();   //en cada acceso a clients --> recurso común

        private Socket sClient; // Atributo que representa el socket del cliente que se conecta al server
        public Socket SClient // Setter y Getter
        {
            set
            {
                sClient = (Socket)value;
            }

            get
            {
                return sClient;
            }
        }


        private IPEndPoint ieCliente; // IPEndPoint del socket del cliente que se quiere conectar al server
        public IPEndPoint IeCliente // Setter y Getter
        {
            set
            {
                ieCliente = (IPEndPoint)SClient.RemoteEndPoint;
            }

            get
            {
                return ieCliente;
            }
        }


        private int numero;
        public int Numero
        {
            set
            {
                numero = value;
            }

            get
            {
                return numero;
            }
        }


        public Cliente(Socket s) // Constructor, donde paso como parámetro el socket
        {
            // Le doy valores al Socket junto al IPEndPoint
            SClient = s;
            IeCliente = (IPEndPoint)s.RemoteEndPoint;

            // Lanzo un hilo 
            Thread t = new Thread(juego);
            t.Start();
        }


        private void juego() // Función que realiza las acciones necesarias para el cliente a la hora de proporcionarle un número aleatorio
        {
            try
            {
                using (NetworkStream ns = new NetworkStream(SClient))
                using (StreamReader sr = new StreamReader(ns))
                using (StreamWriter sw = new StreamWriter(ns))
                {
                    // Defino un mensaje de bienvenida y se lo muestro
                    string bienvenida = "Recibiras un numero del 1 al 20. Si tu numero es el mas alto seras el ganador!";
                    sw.WriteLine(bienvenida);
                    sw.Flush();

                    Numero = Server.asignarNumeroRandom(this); // Le asigno un número random al cliente

                    lock (Server.l)
                    {
                        Monitor.Pulse(Server.l);
                    }

                    while (true) // Mientras este cliente siga conectado...
                    {
                        lock (m)
                        {
                            Monitor.Wait(m);
                        }
                    }
                }
            }
            catch (IOException) // Si ocurre una excepción, el cliente se desconecta
            {
                Server.desconectar(this); 
            }

        }
    }
}
