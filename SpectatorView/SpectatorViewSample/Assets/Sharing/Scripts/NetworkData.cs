﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// If you want to add more network message types:
// 1. Add a new entry to the MessageType enume.
// 2. Add a new Serialize and Deserialize method to this class, following the existing method patterns.
// 3. Call your new deserialize method in DeserializeData if the header matches your new message type.
// 3. Update SharingManager.ReceiveMessage to react to your new message type.
// 4. Update RemotePlayerManager or any relevant game state to react to your new message data.

namespace SimpleSharing
{
    public class NetworkData
    {
        public enum MessageType
        {
            Pose = 0,
            AirTap = 1,
        };

        int WriteHeader(MessageType type)
        {
            int headerSize = sizeof(int);
            Buffer.BlockCopy(BitConverter.GetBytes((int)type), 0, buffer, 0, headerSize);

            return headerSize;
        }

        public const int bufSize = 128;
        public byte[] buffer = new byte[bufSize];

        // Data to serialize
        #region Pose Properties
        public Vector3 Position
        {
            get; private set;
        }

        public Quaternion Rotation
        {
            get; private set;
        }
        #endregion

        #region AirTap Properties
        public Vector3 AirTapLocation
        {
            get; private set;
        }

        public Vector3 AirTapDirection
        {
            get; private set;
        }

        public Vector3 AirTapHitLocation
        {
            get; private set;
        }
        #endregion

        #region Serialize Helpers
        private void SerializeVector(Vector3 input, ref int dstOffset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(input.x), 0, buffer, dstOffset + 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(input.y), 0, buffer, dstOffset + 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(input.z), 0, buffer, dstOffset + 2 * sizeof(float), sizeof(float));
            dstOffset += 3 * sizeof(float);
        }

        private Vector3 DeserializeVector(ref int srcOffset)
        {
            float x = BitConverter.ToSingle(buffer, srcOffset + 0 * sizeof(float));
            float y = BitConverter.ToSingle(buffer, srcOffset + 1 * sizeof(float));
            float z = BitConverter.ToSingle(buffer, srcOffset + 2 * sizeof(float));
            srcOffset += 3 * sizeof(float);

            return new Vector3(x, y, z);
        }

        private void SerializeQuaternion(Quaternion input, ref int dstOffset)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(input.x), 0, buffer, dstOffset + 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(input.y), 0, buffer, dstOffset + 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(input.z), 0, buffer, dstOffset + 2 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(input.w), 0, buffer, dstOffset + 3 * sizeof(float), sizeof(float));
            dstOffset += 4 * sizeof(float);
        }

        private Quaternion DeserializeQuaternion(ref int srcOffset)
        {
            float x = BitConverter.ToSingle(buffer, srcOffset + 0 * sizeof(float));
            float y = BitConverter.ToSingle(buffer, srcOffset + 1 * sizeof(float));
            float z = BitConverter.ToSingle(buffer, srcOffset + 2 * sizeof(float));
            float w = BitConverter.ToSingle(buffer, srcOffset + 3 * sizeof(float));
            srcOffset += 4 * sizeof(float);

            return new Quaternion(x, y, z, w);
        }
        #endregion

        #region Pose
        public int SerializePose(Vector3 position, Quaternion rotation)
        {
            int bufferOffset = WriteHeader(MessageType.Pose);

            // Serialize data to byte array.
            SerializeVector(position, ref bufferOffset);
            SerializeQuaternion(rotation, ref bufferOffset);

            return bufferOffset;
        }

        private void DeSerializePose()
        {
            // Deserialize input byte array into local variables.
            // Assumes input data has same format as Serialize function.
            int bufferOffset = 0;
            Position = DeserializeVector(ref bufferOffset);
            Rotation = DeserializeQuaternion(ref bufferOffset);
        }
        #endregion

        #region AirTap
        public int SerializeAirTap(Vector3 position, Vector3 rotation, Vector3 hitLocation)
        {
            int bufferOffset = WriteHeader(MessageType.AirTap);

            // Serialize data to byte array.
            SerializeVector(position, ref bufferOffset);
            SerializeVector(rotation, ref bufferOffset);
            SerializeVector(hitLocation, ref bufferOffset);

            return bufferOffset;
        }

        private void DeSerializeAirTap()
        {
            // Deserialize input byte array into local variables.
            // Assumes input data has same format as Serialize function.
            int bufferOffset = 0;
            AirTapLocation = DeserializeVector(ref bufferOffset);
            AirTapDirection = DeserializeVector(ref bufferOffset);
            AirTapHitLocation = DeserializeVector(ref bufferOffset);
        }
        #endregion

        public MessageType DeserializeData(byte[] input, int size)
        {
            MessageType header = (MessageType)BitConverter.ToInt32(input, 0);

            if (size > bufSize)
            {
                Debug.LogWarning("Message size is too large.");
                size = bufSize;
            }

            // Strip the leading header bytes.
            Buffer.BlockCopy(input, sizeof(int), buffer, 0, size);

            // Deserialize the network stream to meaningful data.
            switch (header)
            {
                case MessageType.Pose:
                    DeSerializePose();
                    break;
                case MessageType.AirTap:
                    DeSerializeAirTap();
                    break;
            };


            return header;
        }
    }
}