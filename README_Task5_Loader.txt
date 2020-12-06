To run the test for task 4 follow these steps:

1 - Make sure that you are using COM3 to connect to you Arduino, if this com port is unavailable, change the port referenced on line 4 of the Task5.bat file and in the code of the loader found in the Program.cs File.

2 - Make sure that the COM3 port is available (no serial monitor or loader running)

3 - Run the Task5.bat file.

4 - Using the Loader.exe file located in the Loader folder, launch the loader with the path to the program file you are using as the only parameter. If you are not using COM3 as your port, open Program.cs using your C# ide of choice, and change the COM port to whatever you are using, make sure that argument zero is the path to the program file you want to run, and run the program.

5 - Use the loader to ping/transmit/run the file you have selected in the previous step.

6 - VERY IMPORTANT: if the loader stops receiving acks for the packet transmissions as part of a download, stop the loader, and hit the reset button on the arduino. Try again. The unfortunate reality is that packets that have an error in their length field (index 0) will cause the VM to crash, and as reliable as a baud rate of 9600 is, errors are still very possible. To reiterate, in such cases, just restart the arduino, and the Loader, and try again. The system does fail sometimes, but just try again, so you should be good (unless you are very unlucky, have a bad USB cable, or a problem with the arduino itself).