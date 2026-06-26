using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static void Save(string filePath, AudioClip clip)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath)
        );

        using (FileStream stream =
            new FileStream(filePath, FileMode.Create))
        {
            WriteHeader(stream, clip);

            float[] samples =
                new float[clip.samples * clip.channels];

            clip.GetData(samples, 0);

            short[] intData =
                new short[samples.Length];

            byte[] bytesData =
                new byte[samples.Length * 2];

            const float rescale = 32767f;

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] =
                    (short)(samples[i] * rescale);

                BitConverter.GetBytes(intData[i])
                    .CopyTo(bytesData, i * 2);
            }

            stream.Write(
                bytesData,
                0,
                bytesData.Length
            );

            stream.Seek(4, SeekOrigin.Begin);

            stream.Write(
                BitConverter.GetBytes(
                    (int)(stream.Length - 8)
                ),
                0,
                4
            );

            stream.Seek(40, SeekOrigin.Begin);

            stream.Write(
                BitConverter.GetBytes(
                    bytesData.Length
                ),
                0,
                4
            );
        }
    }

    static void WriteHeader(
        FileStream stream,
        AudioClip clip
    )
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;

        stream.Write(
            System.Text.Encoding.UTF8.GetBytes("RIFF"),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes(0),
            0,
            4
        );

        stream.Write(
            System.Text.Encoding.UTF8.GetBytes("WAVE"),
            0,
            4
        );

        stream.Write(
            System.Text.Encoding.UTF8.GetBytes("fmt "),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes(16),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes((ushort)1),
            0,
            2
        );

        stream.Write(
            BitConverter.GetBytes((ushort)channels),
            0,
            2
        );

        stream.Write(
            BitConverter.GetBytes(sampleRate),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes(sampleRate * channels * 2),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes((ushort)(channels * 2)),
            0,
            2
        );

        stream.Write(
            BitConverter.GetBytes((ushort)16),
            0,
            2
        );

        stream.Write(
            System.Text.Encoding.UTF8.GetBytes("data"),
            0,
            4
        );

        stream.Write(
            BitConverter.GetBytes(0),
            0,
            4
        );
    }
}