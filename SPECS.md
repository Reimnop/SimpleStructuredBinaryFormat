# Simple Structured Binary Format Specification

## Overview

The Simple Structured Binary Format is a simple, efficient, structured binary data format. It mimics JSON, with additional types and compression support.

## Unit

All sizes and offsets are in basic machine units (bytes).

## Endianness

All numbers are stored in little-endian format.

## Packing

All fields are tightly packed.

## Header

| Type   | Description                                                           |
|--------|-----------------------------------------------------------------------|
| uint32 | Magic number (default: 0x46425353)                                    |
| bool   | Use compression                                                       |
| Node   | Root node (if compression is enabled, data is compressed with Brotli) |

## Types

## String

| Type    | Description               |
|---------|---------------------------|
| uint8[] | UTF-8 encoded string data |
| uint8   | Null terminator (0x00)    |

## Node

| Type    | Description |
|---------|-------------|
| uint8   | Node type   |
| uint8[] | Node data   |

The following node types are allowed:

| Value | Node type  |
|-------|------------|
| 0x00  | *Reserved* |
| 0x01  | Null       |
| 0x02  | Object     |
| 0x03  | Array      |
| 0x04  | Boolean    |
| 0x05  | SByte      |
| 0x06  | Short      |
| 0x07  | Integer    |
| 0x08  | Long       |
| 0x09  | Byte       |
| 0x0A  | UShort     |
| 0x0B  | UInteger   |
| 0x0C  | ULong      |
| 0x0D  | HalfFloat  |
| 0x0E  | Single     |
| 0x0F  | Double     |
| 0x10  | String     |
| 0x11  | ByteArray  |

## Node data for each node type

### Null

No data.

### Object

| Type          | Description             |
|---------------|-------------------------|
| KeyNodePair[] | Array of key-node pairs |
| uint8         | Null terminator (0x00)  |

#### KeyNodePair

| Type   | Description |
|--------|-------------|
| String | Key         |
| Node   | Node        |

### Array

| Type   | Description            |
|--------|------------------------|
| Node[] | Array of nodes         |
| uint8  | Null terminator (0x00) |

### Boolean

| Type | Description |
|------|-------------|
| bool | Value       |

### SByte

| Type | Description |
|------|-------------|
| int8 | Value       |

### Short

| Type  | Description |
|-------|-------------|
| int16 | Value       |

### Integer

| Type  | Description |
|-------|-------------|
| int32 | Value       |

### Long

| Type  | Description |
|-------|-------------|
| int64 | Value       |

### Byte

| Type  | Description |
|-------|-------------|
| uint8 | Value       |

### UShort

| Type   | Description |
|--------|-------------|
| uint16 | Value       |

### UInteger

| Type   | Description |
|--------|-------------|
| uint32 | Value       |

### ULong

| Type   | Description |
|--------|-------------|
| uint64 | Value       |

### HalfFloat

| Type    | Description |
|---------|-------------|
| float16 | Value       |

### Single

| Type    | Description |
|---------|-------------|
| float32 | Value       |

### Double

| Type    | Description |
|---------|-------------|
| float64 | Value       |

### String

| Type   | Description |
|--------|-------------|
| String | Value       |

### ByteArray

| Type    | Description |
|---------|-------------|
| uint32  | Length      |
| uint8[] | Data        |
