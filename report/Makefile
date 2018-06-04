FILES=$(wildcard src/*.md)

all:
	mkdir tmp
	pandoc                          \
		--from         markdown       \
		--to           latex          \
		--template     template.tex   \
		--out          tmp/report.tex \
		--bibliography lib.bib        \
		--biblatex                    \
		-V links-as-notes             \
		$(FILES)
	pdflatex --output-directory tmp tmp/report.tex
	biber --input-directory tmp --output-directory tmp report
	pdflatex --output-directory tmp tmp/report.tex
	pdflatex --output-directory tmp tmp/report.tex
	mv tmp/report.pdf report.pdf
	rm -r tmp
