// Converter from Arduino serial port bytes to keyboard events

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;

// key stroke simulation API function
public class win32api
{
    [DllImport("user32.dll")]
    public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}

class SerialKeyConverter
{
    static SerialPort serialPort;

    // represents a key state
    enum KeyState
    {
        Pressed, // the key is pressed
        Released // the key is released
    }

    // key assignments for each panel
    private static readonly Keys[] KEY_ASSIGN_LIST = {
        Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.U, Keys.I, Keys.O
    };

    // number of the Arduino panels
    private static readonly int NUM_PANEL = 8;

    // Get the name of the Arduino port from the user input.
    private static string SelectSerialPort()
    {
        // Get an array of available serial port names
        string[] portNames = SerialPort.GetPortNames();

        Console.WriteLine("Select the serial port to which the Arduino is connected.");

        // Display the port names
        int count = 1;
        foreach (string portName in portNames)
        {
            Console.WriteLine("#{0}: {1}", count, portName);
            count++;
        }

        // the port number selected by the user
        int index_serial_port;

        while(true)
        {
            // Request the user to enter the port number
            Console.Write("Please select a number >> ");
            string input = Console.ReadLine();

            // Try parsing to integers, and check that the index is in range
            if (int.TryParse(input, out index_serial_port) && index_serial_port >= 1 && index_serial_port < count)
            {
                // If valid, break from this loop.
                break;
            }
            else
            {
                // If invalid, display the error and ask again.
                Console.WriteLine("Invalid input. Please enter a number in the list.");
            }
        }

        // Return the port name selected by the user
        return portNames[index_serial_port - 1];
    }

    // Establish a connection with the Arduino port
    private static void OpenSerialPort(string serial_port_name)
    {
        serialPort = new SerialPort();
        serialPort.BaudRate = 115200;
        serialPort.Parity = Parity.None;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Handshake = Handshake.None;
        serialPort.PortName = serial_port_name;
        serialPort.Open();
    }

    // Emit a key press event
    private static void PressKey(Keys key)
    {
        win32api.keybd_event((byte)key, 0, 0, (UIntPtr)0);
    }

    // Emit a key release event
    private static void ReleaseKey(Keys key)
    {
        win32api.keybd_event((byte)key, 0, 2, (UIntPtr)0);
    }

    // Emit release events for all-keys
    private static void ReleaseAllKeys()
    {
        for (int i = 0; i < NUM_PANEL; i++)
        {
            ReleaseKey(KEY_ASSIGN_LIST[i]);
        }
    }

    // Main loop for:
    //  Receive panel states from Arduino
    //  Emit key stroke events
    private static void LoopReceive()
    {
        // Create an array of key states and initialize it
        KeyState[] key_state = new KeyState[NUM_PANEL];
        for (int i = 0; i < NUM_PANEL; i++)
        {
            key_state[i] = KeyState.Released;
        }

        // Main loop
        while (true)
        {
            // Read bytes from the port
            byte[] buffer = new byte[256];
            int num_bytes = serialPort.Read(buffer, 0, 256);

            // We may have multiple bytes and use the latest one.
            byte panel_state_data = buffer[num_bytes - 1];

            // Loop for each panel
            for (int i = 0; i < NUM_PANEL; i++)
            {
                // Get a bit that represents the touch status of the panel
                bool is_panel_touched = ((panel_state_data & (1 << i)) != 0);

                if (is_panel_touched)
                {
                    // When the panel is touched, emit a key-press event.
                    if (key_state[i] != KeyState.Pressed)
                    {
                        PressKey(KEY_ASSIGN_LIST[i]);
                    }
                    // Update the keystate
                    key_state[i] = KeyState.Pressed;
                }
                else
                {
                    // When the panel is released, emit a key-release event.
                    if (key_state[i] != KeyState.Released)
                    {
                        ReleaseKey(KEY_ASSIGN_LIST[i]);                       
                    }
                    // Update the keystate
                    key_state[i] = KeyState.Released;
                }
            }
        }
    }
   
    public static int Main(string[] args)
    {
        // Get the name of the Arduino port from the user input
        string serial_port_name = SelectSerialPort();
        try
        {
            // Open the port
            OpenSerialPort(serial_port_name);
        }
        catch (Exception e)
        {
            // Error handling
            Console.WriteLine("Cannot connect to serial port.");
            Console.WriteLine(e);
            return 1;
        }

        // Run the main loop in another thread to read input for quit
        Thread loop_thread = new Thread(new ThreadStart(LoopReceive));
        loop_thread.Start();

        // Receive quit input
        Console.Write("Running. Press enter to quit >> ");
        Console.ReadLine();

        // Termination process after reading the quit input
        // Stop the main loop, release all keys, close the port
        loop_thread.Abort();
        ReleaseAllKeys();
        serialPort.Close();

        return 0;
    }
}
