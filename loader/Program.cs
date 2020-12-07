/* HostSerialLoader.cs
//
// Copyright (C) 2020 by Michel de Champlain
//
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
//#define LoadFromFile;

public class SerialComPort {
    private const byte Ack = 0xCC;
    private const byte Nak = 0x33;
    private const byte PacketSizeMax = 11;

    private enum Status : byte {
        Success = 0x40,
        // Errors during CheckPacket():
        UnknownCmd,
        InvalidCmd,  

        // Errors before command execution:
        InvalidAddr,

        // Errors after command execution:
        MemoryFailed,
        ChecksumInvalid
    }
    private enum Cmd : byte {
        CmdBegin = 0x20,
        Ping = CmdBegin,
        Download,
        Run,
        GetStatus,
        SendData,
        Reset,
        CmdEnd
    }

    // Note: The last zero for each packet is to fullfill the specification
    // of the basic packet format. The Host sends zeros until a non-zero
    // response is received from the target. The Target sends zeros until
    // it is ready to Ack or Nak the packet that has being sent by the Host.
    // See Section 1.1 Basic Packet Format in the Serial Memory Loader
    // document.
    static byte[] pingPacketChecksumInvalid = { 0x03, 0xee, (byte)Cmd.Ping, 0 };
    static byte[] pingPacket = { 0x03, 0x20, (byte)Cmd.Ping, 0 };
    static byte[] getStatusPacket = { 0x03, 0x23, (byte)Cmd.GetStatus, 0 };
    static byte[] sendDataPacket2 = { 0x09, 0xBC, 0x24, 0x91, 0xFF, 0x82, 0xFF, 0x87, 0x00, 0 };
    static byte[] sendDataPacket = { 92 , 209, 0x24, 0xE1, 0x00, 0x25, 0x71, 0xD5, 0x00, 0x2F, 0xFF, 0x85, 0xD5, 0x00, 0x44, 0xFF, 0x85,
        0xD9, 0x09, 0xA8, 0xE0, 0x0E, 0xA0, 0x90, 0x1C, 0xE3, 0x04, 0xE0, 0x09, 0xA0, 0xB4, 0x00, 0xFF,
        0x82, 0xE0, 0xF4, 0xFF, 0x87, 0x03, 0x04, 0xE7, 0xFF, 0xFF, 0xE7, 0xFF, 0xDB, 0x00, 0x54, 0x2E,
        0x53, 0x74, 0x6D, 0x74, 0x00, 0x54, 0x65, 0x73, 0x74, 0x20, 0x31, 0x31, 0x3A, 0x20, 0x62, 0x72,
        0x65, 0x61, 0x6B, 0x20, 0x53, 0x74, 0x61, 0x74, 0x65, 0x6D, 0x65, 0x6E, 0x74, 0x0A, 0x00, 0x39,
        0x38, 0x37, 0x36, 0x35, 0x34, 0x33, 0x32, 0x31, 0x30, 0x0A, 0x00, 0 };
    
    static byte[] runPacket = { 0x03, 0x22, (byte)Cmd.Run, 0 };
    static byte[] resetPacket = { 0x03, (byte)Cmd.Reset, (byte)Cmd.Reset, 0 };

    static bool       _continue;
    static bool       _run;
    static SerialPort _serialPort;

    static byte[] buffer = new byte[12];
    
    // Get the executable code into a formatted byte buffer ready to send with checksum.
    // Example: If a file.exe has a length    of 8  bytes [size(2)          + pgm(6)          ]
    //          then it needs a buffer length of 10 bytes [size|cksm|cmd(3) + pgm(6) + zero(1)] 
    //                                                       0    1    2      3..n-2     n-1
    static byte[] GetCode(string exeFilename) {
        // Read the file into a byte array.
        var fs = new FileStream(exeFilename, FileMode.Open);
        var fileLength  = (int)fs.Length;
        var filePgmSize = fileLength - 2; 
        var bufferLength = 3 + filePgmSize + 1; // [size|cksm|cmd(3) + pgm(6) + zero(1)]
        var buffer = new byte[filePgmSize];

        // Read the first two bytes to skip the size of the executable.
        fs.Read(buffer, 0, 2);

        // Get only the executable code in the buffer starting at index 3.
        fs.Read(buffer, 0, filePgmSize);

        return buffer;
        
        buffer[0] = (byte)bufferLength;
        buffer[2] = (byte)Cmd.SendData;

        byte checksum = buffer[2];
        // Calculate the checksum for range [2..n-1]
        for (int n = 3; n < bufferLength; ++n) {
            checksum += buffer[n]; 
        }
        buffer[1] = checksum;
        return buffer;
    }

    static void transmitCode(byte[] codeBuffer)
    {
        
        
        for (int i = 0; i < codeBuffer.Length; i += 8)//handle the next 8 bytes of code
        {
            List<byte> packetList = new List<byte>();
            
            byte checksum = 0;
            for (int j = i; j < (i + 8) && j < codeBuffer.Length; j++)
            {
                checksum += codeBuffer[j];
                packetList.Add(codeBuffer[j]);
                
            }
            packetList.Add(0);
            packetList.Insert(0,(byte)Cmd.SendData);
            checksum += (byte) Cmd.SendData;
            packetList.Insert(0,checksum);
            packetList.Insert(0,(byte)(packetList.Count));
            byte[] packetArray = packetList.ToArray();
            
            
            Console.Write("\nSending Packet = [");
            for (int x = 0; x < packetArray.Length; x++)
            {
                Console.Write(" <"+packetArray[x]+">");
            }
            Console.Write(" ]\n");
            
            _serialPort.Write(packetArray, 0, packetArray.Length);
            
            Thread.Sleep(2300);//give the target time to handle the packet before sending the next.
        }
    }
    
    // Main thread to transmit packets to target (reading .exe files).
    public static void Main(string[] args)
    {
        string exeFilename;
        if (args.Length != 1) {
            //Console.WriteLine("Usage: HostSerialLoader.exe <file.exe>");

            exeFilename = "C:/Users/Roman/Documents/School/TERM-8/Soen422/project test files/T01.exe";

        }
        else
        {
            exeFilename = args[0];
        }
        
//t        Console.WriteLine("file = {0}", exeFilename);

        // Get the executable code in a buffer to build packet(s).
        var buf = GetCode(exeFilename);
        

        byte[] sendDataPacketFile = new byte[buf.Length];

        for (int n = 0; n < buf.Length; ++n) {
            sendDataPacketFile[n] = buf[n];
        }

    
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        Thread readThread = new Thread(ReadByte);
        // Create a new SerialPort with the same Arduino Nano settings.
        _serialPort = new SerialPort();
        _serialPort.PortName = "COM3";
        _serialPort.BaudRate = 9600; ;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Handshake = Handshake.None;

        // Set the read/write timeouts
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;

        _serialPort.Open();
        _continue = true;
        _run = false;

        // Start a second thread to receive bytes from target.
        readThread.Start();

        Console.WriteLine("Host Serial Loader v1.0 (Cm Virtual Machine on Arduino Nano)");
        //Console.WriteLine("Usage: type 'p'(ping), 'd'(download), 'r'(run), and 'q' to quit.");

        string cmd;

        // Send cmd to target using a command prompt (for debugging purpose).
        
        while (_continue) {
            Console.WriteLine("Usage: type 'ping'(ping), 'status', 'd'(download), 'run', 'reset', and 'quit'");
            Console.Write("$ ");
            cmd = Console.ReadLine();

            if (stringComparer.Equals("quit", cmd)) {
                _continue = false;
            } else if (stringComparer.Equals("ping", cmd)) { // ping
                Console.WriteLine("sending ping");
                _serialPort.Write(pingPacket, 0, 4);
            } else if (stringComparer.Equals("status", cmd)) { // getStatus
                _serialPort.Write(getStatusPacket, 0, 4);
            } else if (stringComparer.Equals("d", cmd)) { // download (sendData - small pgm)
                transmitCode(buf);
            }else if (stringComparer.Equals("reset", cmd)) { // reset packet
                _serialPort.Write(resetPacket, 0, 4);
            } else if (stringComparer.Equals("run", cmd)) { // run
                _serialPort.Write(runPacket, 0, 4);
                _run = true;
            } else {
                _serialPort.Write(pingPacketChecksumInvalid, 0, 4);
            }
        }

        Console.WriteLine("bye!");
        readThread.Join();
        _serialPort.Close();
    }

    // Synchronously reads one byte from the SerialPort input buffer (from target).
    public static void ReadByte()
    {
        bool b = false;
        while (true)
        {
            try
            {
                bool a = true;

                _serialPort.Read(buffer, 0, 1);
                Console.Write("Receiving...\n[\n");
                b = true;
                while (true)
                {
                    if (!a)
                    {
                        _serialPort.Read(buffer, 0, 1);
                    }
                    else
                    {
                        a = false;
                    }



                    if (buffer[0] < 128)
                    {
                        Console.Write((char) buffer[0]);
                    }
                    else
                    {
                        Console.Write(buffer[0]);
                    }
                }



            }
            catch (TimeoutException)
            {
                if (b)
                {
                    b = false;
                    Console.Write("\n]\n");
                }
            }
        }
    }
    public static void ReadByte2() {
        while (_continue) {
            try {
                int size = _serialPort.Read(buffer, 0, 1);
                Console.Write("Receiving:");
                for (int i = 0; i < buffer.Length && buffer[i]!= 0; i++)
                {
                    Console.Write((char)buffer[i]);
                }
                Console.Write("\n");
                
//t                Console.Write("size[" + string.Format("{0:X2}", buffer[0]) + "]:");
                if (buffer[0] != 0) {
                    do {
                        if (!_run && (buffer[0] == Ack)) {
                            size = _serialPort.Read(buffer, 0, 1); // read the zero
                            Console.Write("Ack from target\n$ ");
                            break;
                        }

                        if (_run && (buffer[0] == Ack)) {
                            size = _serialPort.Read(buffer, 0, 1); // read the zero
                            Console.Write("Ack from target. Run!\n");
//t                            Console.Write("it's run + ack/zero " + string.Format("{0:X2} ", buffer[0]));
                            break;
                        }
                        size = _serialPort.Read(buffer, 0, 1);
//t                        Console.Write(string.Format("{0:X2} ", buffer[0]));
                    } while ((buffer[0] != 0)) ;
                }

                if (_run) {
                    while (true) { 
                        size = _serialPort.Read(buffer, 0, 1);

                        if (buffer[0] == Ack) {
                            size = _serialPort.Read(buffer, 0, 1); // read the zero
//t                            Console.Write("running is done + ack/zero " + string.Format("{0:X2} ", buffer[0]));
                            break;
                        }
                        Console.Write((char)buffer[0]);
                    }
                    _run = false;
                    Console.Write("$ ");
                }
            } catch (TimeoutException) { }
        }
    }
}
