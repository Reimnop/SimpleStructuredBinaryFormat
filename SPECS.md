# Simple Structured Binary Format Specification

## Overview

The Simple Structured Binary Format is a simple, efficient, structured binary data format. It mimics JSON, with additional types and compression support.

## Unit

All sizes and offsets are in basic machine units (bytes).

## Endianness

All numbers are stored in little-endian format.

## Header

| Offset | Size | Description                                                    |
|--------|------|----------------------------------------------------------------|
| 0      | 4    | Magic Number (default: 0x46425353)                             |
| 4      | 1    | Compression Mode                                               |
| 5      |      | Root node (compressed with the corresponding compression mode) |

The following compression modes are allowed:

| Value | Mode    |
|-------|---------|
| 0x00  | None    |
| 0x01  | Gzip    |
| 0x02  | Deflate |

## Nodes

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 1    | Node type   |
| 1      |      | Node data   |

The following node types are allowed:

| Value | Type      |
|-------|-----------|
| 0x00  | Null      |
| 0x01  | Object    |
| 0x02  | Array     |
| 0x03  | Boolean   |
| 0x04  | SByte     |
| 0x05  | Short     |
| 0x06  | Integer   |
| 0x07  | Long      |
| 0x08  | Byte      |
| 0x09  | UShort    |
| 0x0A  | UInteger  |
| 0x0B  | ULong     |
| 0x0C  | HalfFloat |
| 0x0D  | Single    |
| 0x0E  | Double    |
| 0x0F  | String    |
| 0x10  | ByteArray |

## Data Representation

### Null

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 0    | No data     |

### Object

| Offset | Size | Description                   |
|--------|------|-------------------------------|
| 0      | 4    | Number of key-value pairs     |

For each key-value pair:

| Offset | Size | Description                   |
|--------|------|-------------------------------|
| 0      | 4    | UTF-8 encoded key name length |
| 4      | n    | UTF-8 encoded key name        |
| 4 + n  |      | Value (other nodes)           |

### Array

| Offset | Size | Description               |
|--------|------|---------------------------|
| 0      | 4    | Number of elements        |
| 4      |      | Elements (other nodes)    |

### Boolean

| Offset | Size | Description               |
|--------|------|---------------------------|
| 0      | 1    | Value (1: true, 0: false) |

### SByte

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 1    | Value       |

### Short

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 2    | Value       |

### Integer

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 4    | Value       |

### Long

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 8    | Value       |

### Byte

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 1    | Value       |

### UShort

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 2    | Value       |

### UInteger

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 4    | Value       |

### ULong

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 8    | Value       |

### HalfFloat

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 2    | Value       |

### Single

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 4    | Value       |

### Double

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 8    | Value       |

### String

| Offset | Size | Description                   |
|--------|------|-------------------------------|
| 0      | 4    | UTF-8 encoded string length   |
| 4      |      | UTF-8 encoded string          |

### ByteArray

| Offset | Size | Description |
|--------|------|-------------|
| 0      | 4    | Length      |
| 4      |      | Data        |
