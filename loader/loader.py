import serial
import cmd
import sys
from packet import packet

class Loader(cmd.Cmd):
    ser = serial.Serial()
    ser.port = 'COM4'
    ser.baudrate = 9600

    def check_response(self, response):
        if response[0] == bytes(204) and response[1] == bytes(0):
            return True
        else:
            return False
        
    def do_send(self, line):
        """Send data to the serial host, commands are: """
        packet_to_send = packet(line)
        try:
            self.ser.open()
            self.ser.write(packet_to_send.get_packet())
            response = self.ser.read(2) #ack should be 2 bytes
            
            if check_response(response):
                self.er.close()
            else:
                self.er.close()
                raise Exception("ACK not reveived")

        except serial.SerialException as e:
            sys.stderr.write('Could not open serial port {}: {}\n'.format(self.ser.name, e))
            return
    
    def do_baud(self, line):
        """Set the baudrate for communications"""
        self.ser.baudrate = int(line)
        print(f"Current config: {self.ser}")

    def do_port(self, line):
        """Set port for communications"""
        self.ser.port = int(line)
        print(f"Current config: {self.ser}")
   

    
    def do_close(self, line):
        """Close the tool"""
        sys.exit(1)


if __name__ == "__main__":
    Loader().cmdloop()