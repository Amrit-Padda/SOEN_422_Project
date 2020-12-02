class packet():
    commands = {'ping': 32, 'getstatus': 35, 'download': 33, 'sendData': 36, 'run': 34, 'reset': 37}
    packet = []

    def __init__(self, command):
        comm = command.split()
        data = []

        if comm[0] == 'download':
            address = comm[1]
            program_size = comm[2]
            data.append(commands[comm[0]])
            data.append(commands[comm[1]])

        checksum = sum(data) #has to be truncated
        size = len(data)  

        self.packet = [size, checksum, data, 0]

    def get_packet(self):
        return bytearray(self.packet)
    



        


    
    