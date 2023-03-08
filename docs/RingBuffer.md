# RingBuffer

A fixed‑size circular buffer that stores the most recent integer values written to it. When the buffer reaches its capacity, new writes overwrite the oldest entry, allowing constant‑time insertion and retrieval of the latest sample.

## API

### RingBuffer(Int32 capacity)

Creates a new instance with the specified storage capacity.

- **capacity** – The maximum number of elements the buffer can hold. Must be greater than zero.
- **Exceptions**  
  - `ArgumentOutOfRangeException` – `capacity` is less than or equal to zero.

### Write(Int32 value)

Adds a value to the buffer. If the buffer is full, the oldest value is discarded.

- **value** – The integer to store.
- **Return** – None.
- **Exceptions** – None.

### ReadLatest() : Int32

Retrieves the most recently written value.

- **Parameters** – None.
- **Return** – The integer that was written last; if no values have been written, the behavior is defined by the implementation (see Notes).
- **Exceptions** –  
  - `InvalidOperationException` – The buffer contains no elements.

### Clear()

Removes all elements from the buffer, resetting it to an empty state.

- **Parameters** – None.
- **Return** – None.
- **Exceptions** – None.

## Usage

```csharp
// Example 1: basic write and read latest
var rb = new RingBuffer(5);
rb.Write(10);
rb.Write(20);
int latest = rb.ReadLatest(); // latest == 20
```

```csharp
// Example 2: using Clear to reset the buffer
var rb = new RingBuffer(3);
rb.Write(1);
rb.Write(2);
rb.Write(3);
rb.Clear(); // buffer is now empty
try
{
    int _ = rb.ReadLatest(); // throws InvalidOperationException
}
catch (InvalidOperationException)
{
    // handle empty buffer
}
```

## Notes

- Reading from an empty buffer throws `InvalidOperationException`; callers should ensure the buffer contains at least one element before invoking `ReadLatest`.
- The constructor validates that capacity is positive; supplying zero or a negative value results in an `ArgumentOutOfRangeException`.
- `Write` operates in O(1) time and never throws due to capacity limits; it silently overwrites the oldest entry when the buffer is full.
- The type does not provide any internal synchronization. Concurrent access from multiple threads requires external locking or other concurrency controls to avoid race conditions.  
- After calling `Clear`, subsequent calls to `ReadLatest` will throw until new values are written.
