FILES=$(wildcard src/*.md)
PFDLATEX_FLAGS=
ifeq ($(DRAFTMODE),1)
	PFDLATEX_FLAGS += -draftmode
endif

.PHONY: all bib clean mkdir pandoc watch

all: mkdir pandoc bib copy

bib:
	biber --input-directory tmp --output-directory tmp report
	pdflatex --output-directory tmp tmp/report.tex
	pdflatex --output-directory tmp tmp/report.tex

clean:
	rm -rf tmp/
	rm report.pdf

copy:
	cp tmp/report.pdf report.pdf

mkdir:
	mkdir -p tmp

pandoc:
	pandoc                          \
		--from         markdown       \
		--to           latex          \
		--template     template.tex   \
		--out          tmp/report.tex \
		--bibliography lib.bib        \
		--biblatex                    \
		-V links-as-notes             \
		$(FILES)
	# this is to make some certain sections blue
	perl -pi -e 's|start(.*)color|{\\color{\1}|' tmp/report.tex
	perl -pi -e 's|end(.*)color|}|' tmp/report.tex
	pdflatex $(PFDLATEX_FLAGS) --output-directory tmp tmp/report.tex

watch:
	watchman-make -p 'src/*.md' '*.tex' -t pandoc copy -p '*.lib' -t all
