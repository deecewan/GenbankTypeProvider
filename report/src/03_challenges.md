# Challenges

As alluded to multiple times previously, optimising for the initial load time,
as well as subsequent child-type loads is a big challenge. This will be tackled
as part of the future work of this project. A more detailed analysis is present
in the following section.

Perhaps the most apparent challenge during development was the inherent
difficulty in developing something that is used by the tool that is developing
it. Somewhat circularly, Visual Studio is the IDE in use to build the type
provider. The type provider is then consumed by Visual Studio's Intellisense
process to build out the completion lists.

Whilst normally this makes little difference, it becomes troublesome when
attempting to re-compile the provider. The Intellisense process takes a lock on
the generated `.dll` file. The net result of that is that Visual Studio can't
compile because the output location of the `.dll` is not writeable.

The current strategy around this depends on the aim. For regular development, a
Debug strategy has been set up on the project to launch a second instance of
Visual Studio. This enables the use of breakpoints and also encapsulates the
lock on the `.dll` to a single, ephemeral instance of the IDE. When the code
needs to be re-compiled, the second instance is closed, thus dropping its lock
on the `.dll`, debugging ends, and the initial instance is free to compile
again.

This falls apart when there are unexpected errors in the type provider. Because
the compiler is not a part of the executing assembly (rather, a part of the
compiling assembly), errors are not logged to the default output console.
Furthermore, even when errors can be captured using breakpoints or similar, they
are quite often impossible to decipher. Enabling debugging symbols did not seem
to have an effect.  In this situation, a console running `fsi.exe` is started,
using the `-r` flag to require the type provider assembly. This also takes a
lock on the `.dll`, but any logging from the type provider appears in the
console window. Unfortunately, error messages are still cryptic in general, so a
common strategy was to place logs around everything, increasing in granularity
until the offending line of code was found.

It is highly likely that the cause of many of the problems with this project is
inexperience. A lack of understanding of type providers, and only rudimentary
understanding of F# made knowing where to start difficult. It slowed progress,
as the same sections of code were re-written time and time again as knowledge of
idiomatic patterns increased.

The advantage to this is that the code that is present is, subjectively, quite
well structured and easy to follow.

Finally, again a lack of understanding of bioinformatics. This challenge has
presented itself with difficulties in understanding Genbank, and will presumably
continue to cause problems into the future, around topics such as adequate
feature sets. A shortcoming of experience may result in the production of a tool
that is not useful to the community.  To mitigate this, consultation with the
community should happen as soon as the base requirements are complete.
