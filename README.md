# digraphs-lookup

When doing interviews, one of the theoritical exercise we gave to potential employees was to find a list of letter pairs in a long string (e.g. a book).

After a few times, I had a few ideas myself of how such a (very useful indeed) algorithm could be written, and then I could not resist anymore and I had to write one.

This implementation ignores disk reading speed and JIT, and focuses on the algorithm itself.

## Results

This runs in `Release` mode, searching for 5 digraphs in Frankenstein, 1k warmup runs and 1k measured runs. Not perfect but good enough.

This is the "naive" implementation (`SubstringPairsDigraphsLookup`). We check every pairs of letters using `substring` and use a dictionary for lookup.

    1000 runs
    16285ms total run time
    Average time: 0.016285
    | Min | 25% | 50% | 75% | 95% | Max |
    | 015 | 016 | 016 | 016 | 017 | 028 |

This is my implementation (`BitShiftingBinarySearchDigraphsLookup`) , which reads one character as a `byte`, and use bit shifting and bit masking to create an integer lookup key. I also use binary search instead of a dictionary (note that a linear search was pretty much the same speed, but with more keys it would have been faster. For a huge amount of digraphs, a dictionary would have been better).

    1000 runs
    1649ms total run time
    Average time: 0.001649
    | Min | 25% | 50% | 75% | 95% | Max |
    | 001 | 001 | 002 | 002 | 002 | 005 |
