using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Ejercicio3
{
    class Cliente
    {
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


        private bool listo;
        public bool Listo
        {
            set
            {
                listo = value;
            }

            get
            {
                return listo;
            }
        }


        public Cliente(Socket s) // Constructor, donde paso como parámetro el socket y le doy valores junto al IPEndPoint
        {
            SClient = s;
            IeCliente = (IPEndPoint)s.RemoteEndPoint;
        }
    }
}
