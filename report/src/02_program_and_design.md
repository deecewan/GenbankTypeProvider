# Program \& Design

The literature review points towards the benefit of making available a type
provider to assist in development using GenBank data.  The plan for this is to
base the work upon another type provider for a similar purpose and bring it up
to a standard where it can be easily used. This includes improving
documentation, which will help in developing a deeper understanding of the
necessary work required to improve the type provider.

Once there is a stable base from which to build, a more thorough type provider
will be built, enabling extensive use of GenBank data through both intellisense
and compile-time information. GenBank is the target data format due to its
relative popularity, as well as having an initial implementation to build from.

Finally, abstracting this provider to a more generic implementation which will
allow for use with more sources of data than GenBank.

The goals of the project are:

- expand upon the existing type provider to improve ease-of-use and
  functionality

- consult with industry professionals to determine what they need from the
  software

- based on the previous feedback, expand the scope of the type provider to
  include broader functionality

- widen the scope of the types provided to allow for more sources than just
  GenBank

- release a type provider to the community to assist in the simplification of
  programmatic access to bioinformatic data

## Steps for completion

Firstly, an in-depth look into the existing type provider (available
[here](https://github.com/jamesmhogan/Experimental-dotnetbioGenBankProvider))
will be done in order to figure out exactly what needs to be changed. This type
provider was completed using the .NET Bio library. .NET Bio provides a range of
parsers and formatters, so an exploration of this library will provide a good
stepping stone to decide what is feasible to complete within the available
timeframe.

As part of understanding the .NET Bio library, it is important to examine the
syntax of the GenBank database format will be required to find out what is
available to extend the project. The GenBank format is a commonly used
format in the industry, so it is critical that it be understood correctly to
build accurate parsers (or to ensure existing parsers are working as intended).
This will involve finding any relevant specifications.

Consult with the community of potential users to determine what is required, and
decide on a feature set for the 'optimal' case solution. The existing
implementation already has some proposed future work that can be used as a
starting point. Given that a goal of this research is to provide a tool to the
community, it is important that the delivered tool actually suits the purposes
required.

Further research on type providers and their creation through way of tutorials
by leaders in the space, such as Tomas Petricek and Don Syme. There are many
learning resources, both written and in video form, that can be used to better
understand type providers at a core level.

