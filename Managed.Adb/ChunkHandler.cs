﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Managed.Adb
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ChunkHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public enum ByteOrder
        {
            /// <summary>
            /// 
            /// </summary>
            LittleEndian,
            /// <summary>
            /// 
            /// </summary>
            BigEndian
        }

        /// <summary>
        /// 
        /// </summary>
        public const int CHUNK_HEADER_LEN = 8;   // 4-byte type, 4-byte len
        /// <summary>
        /// 
        /// </summary>
        public const ByteOrder CHUNK_ORDER = ByteOrder.BigEndian;

        /// <summary>
        /// 
        /// </summary>
        public const int CHUNK_FAIL = -1;

        /// <summary>
        /// Prevents a default instance of the <see cref="ChunkHandler"/> class from being created.
        /// </summary>
        ChunkHandler ( ) { }

        /**
         * Client is ready.  The monitor thread calls this method on all
         * handlers when the client is determined to be DDM-aware (usually
         * after receiving a HELO response.)
         *
         * The handler can use this opportunity to initialize client-side
         * activity.  Because there's a fair chance we'll want to send a
         * message to the client, this method can throw an IOException.
         */
        public abstract void ClientReady ( IClient client );

        /**
         * Client has gone away.  Can be used to clean up any resources
         * associated with this client connection.
         */
        public abstract void ClientDisconnected ( IClient client );

        /**
         * Handle an incoming chunk.  The data, of chunk type "type", begins
         * at the start of "data" and continues to data.limit().
         *
         * If "isReply" is set, then "msgId" will be the ID of the request
         * we sent to the client.  Otherwise, it's the ID generated by the
         * client for this event.  Note that it's possible to receive chunks
         * in reply packets for which we are not registered.
         *
         * The handler may not modify the contents of "data".
         */
        public abstract void HandleChunk ( IClient client, int type,
                byte[] data, bool isReply, int msgId );

        /**
         * Handle chunks not recognized by handlers.  The handleChunk() method
         * in sub-classes should call this if the chunk type isn't recognized.
         */
        protected void handleUnknownChunk ( IClient client, int type,
                MemoryStream data, bool isReply, int msgId )
                {
            if ( type == CHUNK_FAIL )
            {
                int errorCode, msgLen;
                String msg;

                errorCode = data.ReadByte ( );
                msgLen = data.ReadByte ( );

                msg = GetString ( data, msgLen );
                Log.w ( "ddms", "WARNING: failure code=" + errorCode + " msg=" + msg );
            }
            else
            {
                Log.w ( "ddms", "WARNING: received unknown chunk " + name ( type )
                        + ": len=" + data.Length + ", reply=" + isReply
                        + ", msgId=0x" + msgId.ToString ( "X8" ) );
            }
            Log.w ( "ddms", "         client " + client + ", handler " + this );
        }


        /**
         * Utility function to copy a String out of a ByteBuffer.
         *
         * This is here because multiple chunk handlers can make use of it,
         * and there's nowhere better to put it.
         */
        static String GetString ( MemoryStream buf, int len )
        {
            char[] data = new char[len];
            for ( int i = 0; i < len; i++ )
                data[i] = (char)buf.ReadByte ( );
            return new String ( data );
        }

        /**
         * Utility function to copy a String into a ByteBuffer.
         */
        static void PutString ( MemoryStream buf, String str )
        {
            byte[] data = Encoding.Default.GetBytes ( str );
            buf.Write ( data, 0, data.Length );
        }

        /**
         * Convert a 4-character string to a 32-bit type.
         */
        static int type ( String typeName )
        {
            int val = 0;

            if ( typeName.Length != 4 )
            {
                Log.e ( "ddms", "Type name must be 4 letter long" );
                throw new ArgumentException ( "Type name must be 4 letter long" );
            }

            for ( int i = 0; i < 4; i++ )
            {
                val <<= 8;
                val |= (byte)typeName[i];
            }

            return val;
        }

        /**
         * Convert an integer type to a 4-character string.
         */
        static String name ( int type )
        {
            char[] ascii = new char[4];

            ascii[0] = (char)( ( type >> 24 ) & 0xff );
            ascii[1] = (char)( ( type >> 16 ) & 0xff );
            ascii[2] = (char)( ( type >> 8 ) & 0xff );
            ascii[3] = (char)( type & 0xff );

            return new String ( ascii );
        }

        /**
         * Allocate a ByteBuffer with enough space to hold the JDWP packet
         * header and one chunk header in addition to the demands of the
         * chunk being created.
         *
         * "maxChunkLen" indicates the size of the chunk contents only.
         */
        static BinaryReader allocBuffer ( int maxChunkLen )
        {
            /*
            using ( MemoryStream ms = new MemoryStream(JdwpPacket.JDWP_HEADER_LEN + 8 +maxChunkLen) ) {
                using ( EndianBinaryReader ebr = new EndianBinaryReader(CHUNK_ORDER == ByteOrder.LittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big, ms ) ) {
                    return ebr;
                }
            }
             */
            return null;
        }

        /**
         * Return the slice of the JDWP packet buffer that holds just the
         * chunk data.
         */
        static BinaryReader GetChunkDataBuf ( MemoryStream jdwpBuf )
        {
            /* EndianBinaryReader ebr = null;

             System.Diagnostics.Debug.Assert ( jdwpBuf.Position == 0 );

             jdwpBuf.Position = JdwpPacket.JDWP_HEADER_LEN + CHUNK_HEADER_LEN;
         EndianBitConverter ebc = null;
         if ( CHUNK_ORDER == ByteOrder.LittleEndian ) {
             ebc = EndianBitConverter.Little;
         } else {
             ebc =  EndianBitConverter.Big;
         }
             ebr = new EndianBinaryReader(ebc,new MemoryStream(jdwpBuf.ToArray()) );
             jdwpBuf.Position = 0;
             return ebr;*/

            return null;
        }

        /**
         * Write the chunk header at the start of the chunk.
         *
         * Pass in the byte buffer returned by JdwpPacket.getPayload().
         */
        // TODO: JdwpPacket
        static void finishChunkPacket (/*JdwpPacket*/ Object packet, int type, int chunkLen )
        {
            /*ByteBuffer buf = packet.getPayload();

            buf.putInt(0x00, type);
            buf.putInt(0x04, chunkLen);

            packet.finishPacket(CHUNK_HEADER_LEN + chunkLen);*/
        }

        /**
         * Check that the client is opened with the proper debugger port for the
         * specified application name, and if not, reopen it.
         * @param client
         * @param uiThread
         * @param appName
         * @return
         */
        protected static IClient checkDebuggerPortForAppName ( IClient client, String appName )
        {
            /*IDebugPortProvider provider = DebugPortManager.getProvider ( );
            if ( provider != null ) {
                Device device = client.GetDeviceImpl ( );
                int newPort = provider.GetPort ( device, appName );

                if ( newPort != IDebugPortProvider.NO_STATIC_PORT &&
                                newPort != client.getDebuggerListenPort ( ) ) {

                    AndroidDebugBridge bridge = AndroidDebugBridge.Bridge;
                    if ( bridge != null ) {
                        DeviceMonitor deviceMonitor = bridge.DeviceMonitor;
                        if ( deviceMonitor != null ) {
                            deviceMonitor.addClientToDropAndReopen ( client, newPort );
                            client = null;
                        }
                    }
                }
            }*/

            return client;
        }
    }
}
