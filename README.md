# digraphs-lookup

When doing interviews, one of the theoritical exercise we gave to potential employees was to find a list of letter pairs in a long string (e.g. a book).

After a few times, I had a few ideas myself of how such a (very useful indeed) algorithm could be written, and then I could not resist anymore and I had to write one.

This implementation ignores disk reading speed and JIT, and focuses on the algorithm itself.

## Hall of Fame

1. [Socolin](https://github.com/Socolin) for `MemoryArraySearchDigraphsLookupV2`
2. [christianrondeau](https://github.com/christianrondeau) for `BitShiftingBinarySearchDigraphsLookup`

## Performance Test

This runs in `Release` mode, searching for 5 digraphs in Frankenstein, 1k warmup runs and 1k measured runs. Not perfect but good enough.

## Results

This is the "naive" implementation (`SubstringPairsDigraphsLookup`). We check every pairs of letters using `substring` and use a dictionary for lookup.

    Average time: 5497 µs
    Percentiles (in microseconds):
    |   Min |   25% |   50% |   75% |   95% |   Max |
    |  5227 |  5410 |  5480 |  5563 |  5727 |  7425 |

This is my implementation (`BitShiftingBinarySearchDigraphsLookup`), which reads one character as a `byte`, and use bit shifting and bit masking to create an integer lookup key. I also use binary search instead of a dictionary (note that a linear search was pretty much the same speed, but with more keys it would have been faster. For a huge amount of digraphs, a dictionary would have been better).

    Average time: 0699 µs
    Percentiles (in microseconds):
    |   Min |   25% |   50% |   75% |   95% |   Max |
    |   635 |   660 |   674 |   714 |   833 |  1030 |

This is @Socolin's solution, which creates an array large enough to cover all possible ASCII digraphs. This replaces a few arithmetic operations by a larger memory space, making this the fastest solution!

    Average time: 0354 µs
    Percentiles (in microseconds):
    |   Min |   25% |   50% |   75% |   95% |   Max |
    |   322 |   336 |   342 |   355 |   416 |   662 |
