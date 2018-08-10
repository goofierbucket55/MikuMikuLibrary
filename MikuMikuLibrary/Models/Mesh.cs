﻿using MikuMikuLibrary.IO.Common;
using MikuMikuLibrary.IO.Sections;
using MikuMikuLibrary.Materials;
using System.Collections.Generic;

namespace MikuMikuLibrary.Models
{
    public class Mesh
    {
        public const int ByteSize = 0x50;

        public List<SubMesh> SubMeshes { get; }
        public List<Material> Materials { get; }
        public MeshSkin Skin { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        internal void Read( EndianBinaryReader reader, MeshSection section = null )
        {
            uint signature = reader.ReadUInt32();
            BoundingSphere = BoundingSphere.FromReader( reader );
            int subMeshCount = reader.ReadInt32();
            uint subMeshesOffset = reader.ReadUInt32();
            int materialCount = reader.ReadInt32();
            uint materialsOffset = reader.ReadUInt32();

            SubMeshes.Capacity = subMeshCount;
            for ( int i = 0; i < subMeshCount; i++ )
            {
                reader.ReadAtOffset( subMeshesOffset + ( i * SubMesh.ByteSize ), () =>
                {
                    var submesh = new SubMesh();
                    submesh.Read( reader, section );
                    SubMeshes.Add( submesh );
                } );
            }

            Materials.Capacity = materialCount;
            for ( int i = 0; i < materialCount; i++ )
            {
                reader.ReadAtOffset( materialsOffset + ( i * Material.ByteSize ), () =>
                {
                    var material = new Material();
                    material.Read( reader );
                    Materials.Add( material );
                } );
            }
        }

        internal void Write( EndianBinaryWriter writer, MeshSection section = null )
        {
            writer.Write( 0x10000 );
            BoundingSphere.Write( writer );
            writer.Write( SubMeshes.Count );
            writer.EnqueueOffsetWriteAligned( 4, AlignmentKind.Left, () =>
            {
                foreach ( var subMesh in SubMeshes )
                    subMesh.Write( writer, section );
            } );
            writer.Write( Materials.Count );
            writer.EnqueueOffsetWriteAligned( 4, AlignmentKind.Left, () =>
            {
                foreach ( var material in Materials )
                    material.Write( writer );
            } );
            writer.WriteNulls( 0x28 );
        }

        public Mesh()
        {
            SubMeshes = new List<SubMesh>();
            Materials = new List<Material>();
        }
    }
}
