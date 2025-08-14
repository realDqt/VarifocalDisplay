using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class TunableLensController : MonoBehaviour
{
    [Header("Connection Settings")]
    public string portName = "COM5";
    private SerialPort serialPort;

    [Header("Sinusoid Parameters")]
    [Tooltip("Frequency of the sine wave in Hz.")]
    public float frequency = 8.0f;

    [Tooltip("Amplitude of the sine wave in dpt.")]
    public float amplitude = 6f;

    [Tooltip("Offset of the sine wave in dpt.")]
    public float offset = 0f;

    [Tooltip("Phase delay in degrees (��).")]
    public float phase = 0f;

    [Tooltip("Number of cycles for the animation. 0 for infinite.")]
    public int cycles = 10;

    [Header("Replay Settings")]
    [Tooltip("ÿ���Լ���Ͷ�����ָ��")]
    public float commandsPerSecond = 12.0f;

    public bool IsConnected => serialPort != null && serialPort.IsOpen;

    void OnApplicationQuit() => Close();
    void OnDisable() => Close();

    private readonly List<byte[]> commandSequence = new List<byte[]>()
    {
        // Polling / Status Check Commands
        new byte[] { 0x7e, 0x00, 0x11, 0x02, 0xc8, 0x0f, 0x6d, 0x7c, 0x7e },
        new byte[] { 0x7e, 0x00, 0x11, 0x02, 0xc8, 0x0e, 0x4c, 0x6c, 0x7e },
        new byte[] { 0x7e, 0x00, 0x11, 0x02, 0xc8, 0x01, 0xa3, 0x9d, 0x7e },

        // Set Number of Cycles to 5
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x07, 0x00, 0x00, 0x00, 0x05, 0x8a, 0xec, 0x7e },

        // Other Signal Generator Parameter Settings
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x40, 0x00, 0x00, 0x00, 0x00, 0x60, 0x55, 0x82, 0x7e },
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x0e, 0x3f, 0x00, 0x00, 0x00, 0x54, 0xec, 0x7e },
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x02, 0xbe, 0x7e },

        // The Main Composite "Setup" Command for the Sine Wave
        new byte[] { 0x7e, 0x00, 0x12, 0x32, 0x00, 0x08,
            0x60, 0x04, 0x60, 0x03, 0x60, 0x05, 0x60, 0x06,
            0x60, 0x02, 0x60, 0x00, 0x60, 0x01, 0x60, 0x08,
            0x40, 0xa0, 0x00, 0x00,
            0x41, 0xa0, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x03,
            0x00, 0x00, 0x00, 0x00,
            0x3f, 0x00, 0x00, 0x00,
            0xb3, 0x28, 0x7e },

        // More Signal Generator Parameter Settings
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x09, 0x00, 0x00, 0x00, 0x00, 0x87, 0x73, 0x7e },
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x0c, 0x00, 0x00, 0x00, 0x00, 0xd0, 0x50, 0x7e },

        // Another Polling Command
        new byte[] { 0x7e, 0x00, 0x11, 0x02, 0x60, 0x07, 0xb2, 0x69, 0x7e },

        // The "Start" Command
        new byte[] { 0x7e, 0x00, 0x10, 0x06, 0x60, 0x01, 0x00, 0x00, 0x00, 0x01, 0x8b, 0x61, 0x7e }
    };

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            serialPort.Open();
            Debug.Log($"[TunableLensController] Started on {portName} at 115200 baud. Replaying {commandSequence.Count} commands at approx. {commandsPerSecond} Hz.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TunableLensController] Error: {ex.Message}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartSinusoidAnimation(cycles, amplitude, frequency, offset, phase);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetFocalPower(offset);
        }
    }

    public void Connect()
    {
        if (IsConnected)
        {
            Debug.LogWarning("[LensController] Already connected.");
            return;
        }

        try
        {
            serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 200;
            serialPort.Open();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TunableLensController] Failed to open port {portName}: {ex.Message}");
        }
    }

    public void Close()
    {
        if (IsConnected)
        {
            Debug.Log("[LensController] Closing serial port.");
            serialPort.Close();
        }
    }

    public void SetFocalPower(float diopter)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[LensController] Not connected. Cannot set focal power.");
            return;
        }

        StartSinusoidAnimation(10, 0f, 0.001f, diopter, 0f);

    }

    public void StartSinusoidAnimation(int cycle, float amp, float freq, float off, float phaseDeg)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[LensController] Not connected. Cannot start animation.");
            return;
        }
        // Payload Data
        Array.Copy(IntToBytes(cycle), 0, commandSequence[3], 6, 4);
        byte[] crcData_cycle = new byte[9];
        Array.Copy(commandSequence[3], 1, crcData_cycle, 0, 9);
        ushort crc = CRC16IBM.ComputeChecksum(crcData_cycle);
        commandSequence[3][10] = (byte)(crc & 0xFF); // CRC Low Byte
        commandSequence[3][11] = (byte)(crc >> 8);   // CRC High Byte
        Debug.Log($"commandSequence[3]: {BitConverter.ToString(commandSequence[3]).Replace("-", " ")}");

        Array.Copy(FloatToBytes(amp), 0, commandSequence[7], 22, 4);
        Array.Copy(FloatToBytes(freq), 0, commandSequence[7], 26, 4);
        Array.Copy(FloatToBytes(off), 0, commandSequence[7], 30, 4);
        Array.Copy(FloatToBytes(phaseDeg * Mathf.Deg2Rad), 0, commandSequence[7], 34, 4);
        byte[] crcData_set = new byte[53];
        Array.Copy(commandSequence[7], 1, crcData_set, 0, 53);
        crc = CRC16IBM.ComputeChecksum(crcData_set);
        commandSequence[7][54] = (byte)(crc & 0xFF); // CRC Low Byte
        commandSequence[7][55] = (byte)(crc >> 8);   // CRC High Byte
        Debug.Log($"commandSequence[7]: {BitConverter.ToString(commandSequence[7]).Replace("-", " ")}");

        SendBytes();
    }

    private void SendBytes()
    {
        int interCommandDelay = (int)(1000 / commandsPerSecond);
        if (!IsConnected) return;
        try
        {
            foreach (byte[] commandToSend in commandSequence)
            {
                serialPort.Write(commandToSend, 0, commandToSend.Length);
                Thread.Sleep(interCommandDelay);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LensController] Error writing bytes: {ex.Message}");
        }
    }

    private byte[] FloatToBytes(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
    private byte[] IntToBytes(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
}

public static class CRC16IBM
{
    private const ushort Polynomial = 0x1021; // CCITT-FALSE
    private const ushort InitialValue = 0xFFFF;

    public static ushort ComputeChecksum(byte[] data)
    {
        ushort crc = InitialValue;

        foreach (byte b in data)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ Polynomial);
                else
                    crc <<= 1;
            }
        }

        return crc;
    }

    public static byte[] ComputeChecksumBytes(byte[] data)
    {
        ushort crc = ComputeChecksum(data);
        return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
    }
}
