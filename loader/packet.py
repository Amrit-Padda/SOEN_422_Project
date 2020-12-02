class packet():
    size = 0
    checksum = 0
    data = 0
    commands = {'ping': 32, 'getstatus': 35, 'download': 33, 'sendData': 36, 'run': 34, 'reset': 37}
    packet = []

    def __init__(self, command):
        #for now this only supports 1 command at a time
        self.data = self.commands[command]
        self.checksum = self.data
        self.size = 3   #hardcoded for now

        self.packet = [self.size, self.checksum, self.data, 0]

    def get_packet(self):
        return bytearray(self.packet)
    



        


    
    