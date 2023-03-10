# RingBufferExtensions

The `RingBufferExtensions` static class provides a suite of helper methods designed to simplify interactions with circular buffer data structures within the `skia-scope` project. These extension methods offer a concise interface for writing data, retrieving the complete buffer contents, and querying the current occupancy state of the buffer.

## API

### Write
Writes a single floating-point value to the buffer. If the buffer is full, the behavior depends on the underlying implementation, but typically this may throw an exception or overwrite the oldest data.
- **Parameters:**
  - `buffer`: The `RingBuffer<float>` instance.
  - `value`: The `float` value to write.
- **Return Value:** `void`
- **Throws:** Potential exceptions if the buffer implementation enforces strict capacity limits and it is full.

### ReadAll
Retrieves all current values stored in the buffer as a new `float` array. The array length corresponds to the current number of elements in the buffer.
- **Parameters:**
  - `buffer`: The `RingBuffer<float>` instance.
- **Return Value:** A `float[]` containing all current values in the buffer.

### TryWrite
Attempts to write a single floating-point value to the buffer.
- **Parameters:**
  - `buffer`: The `RingBuffer<float>` instance.
  - `value`: The `float` value to write.
- **Return Value:** `bool`: `true` if the write operation was successful; `false` if the buffer was full and the value could not be written.

### IsEmpty
Checks if the buffer contains no elements.
- **Parameters:**
  - `buffer`: The `RingBuffer<float>` instance.
- **Return Value:** `bool`: `true` if the buffer is empty; otherwise `false`.

### IsFull
Checks if the buffer has reached its maximum capacity.
- **Parameters:**
  - `buffer`: The `RingBuffer<float>` instance.
- **Return Value:** `bool`: `true` if the buffer is full; otherwise `false`.

## Usage

```csharp
// Example 1: Basic write and check
var buffer = new RingBuffer<float>(10);

if (!buffer.IsFull()) {
    buffer.Write(1.23f);
}

float[] data = buffer.ReadAll();
```

```csharp
// Example 2: Safe writing loop
var buffer = new RingBuffer<float>(5);
float[] valuesToWrite = { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f };

foreach (var val in valuesToWrite) {
    if (!buffer.TryWrite(val)) {
        Console.WriteLine("Buffer full, skipping value: " + val);
    }
}
```

## Notes

- **Thread Safety:** These extension methods do not provide internal thread synchronization. If the `RingBuffer` instance is accessed by multiple threads simultaneously, the caller must implement appropriate locking mechanisms to ensure data integrity.
- **Edge Cases:** 
    - `ReadAll` on an empty buffer typically returns an empty array.
    - The behavior of `Write` when the buffer is full (e.g., throwing an exception versus overwriting) is dictated by the specific `RingBuffer` implementation being extended; refer to the concrete implementation's documentation.
