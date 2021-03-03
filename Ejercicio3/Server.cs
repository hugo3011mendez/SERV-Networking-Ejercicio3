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
        static readonly internal object l = new object(); // Creo el objeto del lock, porque los hilos están usando recursos iguales
        static List<Cliente> clientes = new List<Cliente>(); // Creo una colección de clientes para ir guardando en ella los clientes que se conectan

        private static List<int> numeros = new List<int>(); // Lista de números que han tocado
        static Random generador = new Random(); // Generador de números aleatorios

        private static DateTime tiempo; // Tiempo transcurrido
        const int TIEMPOJUEGO = 15; // Constante que indica la duración de la cuenta atrás
        private static int seg = TIEMPOJUEGO;
        private static bool final = false;


        internal static int asignarNumeroRandom(Cliente cl) // Le asigno un número entre 1 y 20 al cliente indicado como parámetro
        {
            bool repetir = true; // Booleana para saber si se repite la asignación o no
            bool numAsignado = false; // Booleana que comprueba si ya se le ha asignado un número aleatorio al cliente

            int num = 1; // Número a asignar

            while (repetir)
            {
                num = generador.Next(1, 21); // Le doy un valor aleatorio dentro del rango que queremos

                lock (l)
                {
                    if (!numeros.Contains(num)) // Si la colección de números no tiene a este número...
                    {
                        repetir = false; // Ya no hace falta repetir la asignación del número
                        numeros.Add(num); // Áñado el número a la colección
                        numAsignado = true; // El número ha sido asignado correctamente
                    }
                    if (numAsignado) // Si se ha asignado correctamente el número...
                    {
                       cl.Numero = num; // Establezco el número asignado en su propiedad correspondiente en el cliente
                       //mensajeAUno(c, "Tu numero es " + c.Numero);
                    }
                }
            }

            return cl.Numero; // Devuelvo el número asignado al cliente
        }


        private static void cuentaAtras() // Función donde realizo la cuenta atrás
        {
            tiempo = DateTime.Now.AddSeconds(seg + 1); // Añado un segundo de cada vez al tiempo transcurrido

            while (!final) // Mientras no llegue el final de la cuenta atrás...
            {
                lock (l)
                {
                    if (!final) // Si la cuenta atrás aún no ha finalizado...
                    {
                        DateTime now = DateTime.Now; // Declaro y establezco el valor de una variable como el momento actual
                        TimeSpan dif = tiempo - now; // Declaro y establezco el valor de una variable como la diferencia de tiempo entre el tiempo transcurrido y el momento actual

                        // Si la diferencia de tiempo entre el tiempo transcurrido y el momento actual se puede dividir en segundos y es igual al tiempo restante para que acabe la cuenta atrás
                        if (dif.Milliseconds % 1000 == 0 && dif.Seconds == seg) 
                        {
                            string tiempoRestante = string.Format($"{dif:mm\\:ss}"); // Formateo el tiempo restante en una cadena
                            mensajeATodos(tiempoRestante); // Y se la muestro a todos los clientes

                            seg --; // Reduzco los segundos restantes para acabar la cuenta atrás
                        }


                        if (dif.Seconds == 0) // Si no existe diferencia de tiempo entre el tiempo transcurrido y el momento actual...
                        {
                            string tiempoRestante = string.Format($"{dif:mm\\:ss}"); // Formateo el tiempo restante en una cadena
                            mensajeATodos(tiempoRestante); // Y se la muestro a todos los clientes

                            mensajeATodos("SE ACABO EL TIEMPO!!"); // Mando el mensaje a todos los clientes de que se ha acabado el tiempo
                            comprobarGanador(); // Compruebo el ganador
                            finalJuego(); // Y acabo la partida
                        }
                    }
                }
            }
        }


        private static void mensajeATodos(string msg) // Envío un  mensaje a todos los clientes desde el servidor
        {
            if (msg != null)
            {
                lock (l)
                {
                    for (int i = 0; i < clientes.Count; i++)
                    {
                        try
                        {
                            using (NetworkStream ns = new NetworkStream(clientes[i].SClient))
                            using (StreamReader sr = new StreamReader(ns))
                            using (StreamWriter sw = new StreamWriter(ns))
                            {
                                sw.WriteLine(msg);
                                sw.Flush();
                            }
                        }
                        catch (IOException) // Si ocurre una excepción, desconecto al cliente que tenga el error y muestro el mensaje de error
                        {
                            desconectar(clientes[i]);

                            Console.WriteLine("Error mensajeando a todos los clientes : " + msg);
                        }
                    }
                }
            }
        }


        private static void mensajeAUno(Cliente cl, string msg) // Envío un mensaje a un cliente desde el servidor
        {
            if (msg != null)
            {
                lock (l)
                {
                    try
                    {
                        using (NetworkStream ns = new NetworkStream(cl.SClient))
                        using (StreamReader sr = new StreamReader(ns))
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            sw.WriteLine(msg);
                            sw.Flush();
                        }
                    }
                    catch (IOException) // Si ocurre una excepción, desconecto al cliente correspondiente
                    {
                        Console.WriteLine("Error mensajeando al cliente\t" + cl.IeCliente.Address + ":" + cl.IeCliente.Port);

                        desconectar(cl);
                    }
                }
            }
        }


        // Desconecto al cliente indicado como parámetro cerrando su socket, eliminando su número de la colección y eliminándolo de la colección de clientes
        internal static void desconectar(Cliente cl) 
        {
            lock (l)
            {
                cl.SClient.Close();

                numeros.Remove(cl.Numero);

                clientes.Remove(cl);
            }
        }


        static void comprobarGanador()
        {

            if (clientes.Count > 0)
            {
                Cliente ganador = clientes[0]; // Primero determino al primer cliente como ganador

                for (int i = 0; i < clientes.Count; i++) // Recorro todos los clientes conectados
                {
                    mensajeAUno(clientes[i], "Tu numero es " + clientes[i].Numero); // Muestro su propio número al cliente correspondiente

                    if (clientes[i].Numero >= ganador.Numero) // Si el número del cliente por el que está pasando es mayor al número del cliente establecido como ganador... 
                    {
                        ganador = clientes[i]; // El cliente actual se establece como ganador
                    }
                }


                for (int i = 0; i < clientes.Count; i++) // Vuelvo a recorrer los clientes conectados
                {
                    if (clientes[i] != ganador) // Compruebo si el cliente actual no es el ganador
                    {
                        mensajeAUno(clientes[i], "El numero ganador es : " + ganador.Numero);

                        mensajeAUno(clientes[i], "No has ganado"); // Mando el mensaje de que no ha ganado al cliente actual
                    }
                }

                mensajeAUno(ganador, "HAS GANADO!!"); // Mando el mensaje de que ha ganado al cliente ganador
            }
        }

        private static void finalJuego() // Acciones a realizar cuando se acaba el juego
        {
            if (clientes.Count > 0)
            {
                for (int i = 0; i < clientes.Count; i++) // Recorro la colección de clientes y los desconecto
                {
                    desconectar(clientes[i]);
                }

                seg = TIEMPOJUEGO; // Reinicio la variable seg
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
                Thread hiloCliente = new Thread(cuentaAtras);
                hiloCliente.Start(clientes[clientes.Count - 1]);
            }
        }
    }
}
