using UnityEngine;
using System.Net;
using System.Net.Sockets;

namespace BlenderVirtualMotionCamera
{
    /// <summary>
    /// Sends camera Transform (Position & Rotation) as raw floats via UDP.
    /// Matches the format expected by the Blender VMC addon.
    /// </summary>
    public class OscSender : MonoBehaviour
    {
        [Header("Network Settings")]
        public string ipAddress = "127.0.0.1"; // Set to your PC's IP if testing on mobile
        public int port = 8000;

        private UdpClient _udpClient;
        private IPEndPoint _endPoint;

        void Start()
        {
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        void LateUpdate()
        {
            SendTrackingData();
        }

        void SendTrackingData()
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            // Prepare 7 floats: px, py, pz, rw, rx, ry, rz
            byte[] data = new byte[28];
            
            // Note: Blender script expects Big Endian ('!') and raw floats
            // px, py, pz
            WriteFloat(data, 0, pos.x);
            WriteFloat(data, 4, pos.y);
            WriteFloat(data, 8, pos.z);
            // rw, rx, ry, rz
            WriteFloat(data, 12, rot.w);
            WriteFloat(data, 16, rot.x);
            WriteFloat(data, 20, rot.y);
            WriteFloat(data, 24, rot.z);

            _udpClient.Send(data, data.Length, _endPoint);
        }

        void WriteFloat(byte[] target, int offset, float value)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes); // Convert to Big Endian
            }
            System.Array.Copy(bytes, 0, target, offset, 4);
        }

        void OnDestroy()
        {
            _udpClient?.Close();
        }
    }
}
