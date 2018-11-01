# Future Work

In a similar theme to the previous submission, the work of most importance is the implementation of Genbank file parsing. Whilst progress has been made in this regard, there is still a ways to go. Establishing some use-cases and deciding on relevant metadata will help guide this aspect of the work.

Again, converting the project to 'Project Scaffold', and along with that conversion, adding unit tests. Unit tests will provide guarantees to consumers that the functionality works, and will continue to work. Unit tests also allow for the project to move at a faster pace without the fear of breaking existing functionality. Included in 'Project Scaffold' is the `Expecto` testing library, specifically designed for F#.

Developing out a sample application is also a must, in order to demonstrate to users exactly the power and functionality available. A start has been made on this, but it currently does nothing other than print a few string to the console.

Finding a solution to the `Colorful.Console` issue when building for NuGet will enable the build to be updated more regularly.

As spoken about in the previous project report, search functionality is an important addition that should be implemented. This will broaden the audience to those who aren't looking to explore data, but to process it. These features include loading local Genbank files, searching by genome name, accession number, or some other characteristic.

A shift in priorities away from adding small features to the provider, and
making the provider easier to work on and more stable must be considered. The
backlog of tasks is growing at a rate too large to complete in the given
time-frame, so it is important that, at the very least, there is a strong base
to continue work on in the future.

# Self Reflection

Once again, time management continues to be a cause for concern. Prioritisation of tasks is often wrong. I'll spend too long working on something that has little to no benefit to the project, and neglect parts of the application that could move very quickly with a little bit of work. The logging library, as useful as it is, is a good example of this. It has probably seen close to the same amount of time as the rest of the project combined.

A general lack of understanding of the platform I'm working on makes it difficult to diagnose issues. For an experience person, it may take 10 minutes, or an hour to solve an issue, whereas it will take me hours of research to likely still not come to a good result.  This is evidenced by problems in the build pipeline, though I imagine that confusion over how .NET actually compiles is not a problem unique to me.

It is very evident that the timelines laid out in the project report were
blatantly inaccurate. Estimation of deadlines has never been a strong suit of
mine.  The abundance of red on the updated Gantt chart is evidence of that.
