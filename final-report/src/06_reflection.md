# Reflection

These are some personal reflections from my time working on this project. They
include things I have learned, things I did poorly, and things I can do better
next time.

## First Principles is good

Given the number of re-writes and new architectures this project had, to end up
with essentially the same architecture as I started with is highly frustrating.
It has shown me that the 'industry standards' exist for a reason - they are
pretty good ways of doing things. The more important thing that I've learned is
that I should always go back and make sure I have a fundamental understanding of
what is going on before allowing someone else to abstract it away. Not
understanding these things has made almost every part of the project more
difficult.

## Keep it simple

Trying to abstract away too much, as with the caching, or trying to optimise
prematurely has resulted in a slower development cycle for entirely marginal
gains. Spending 1+ hours on trying to best optimise a looping code path, where
it may iterate 10 times, is a poor use of time (see [below](#time-management)).

Keeping it simple, and going with the obvious, easy solution to start with would
have resulted in a better product for the consumer, and very minor degradation
in performance (there are many other performance problems that have a much
higher impact on the user).

## Time Management

Once again, time management continues to be a cause for concern.
Prioritisation of tasks is often wrong. I'll spend too long working on
something that has little to no benefit to the project, and neglect parts of
the application that could move very quickly with a little bit of work.

This has been a problem for the majority of my university career, and I believe
that it's partially because I work well under pressure. Like a gas, I expand the
length of a project to fill the entire amount of time allotted, and end up doing
the bulk of the work at the tail end of the project.

It is very evident that the timelines laid out in the project report were
blatantly inaccurate. Estimation of deadlines has never been a strong suit of
mine. The abundance of red on the Gantt charts previously submitted is evidence
of that.