using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using RabotoraX.Core.Cryptography;
using RabotoraX.Core.Resources;

namespace RabotoraX.Utility.Packer.Models;

public sealed class PackItem
{
    public string LogicalPath { get; set; } = string.Empty;
    public string PhysicalPath { get; set; } = string.Empty;
}