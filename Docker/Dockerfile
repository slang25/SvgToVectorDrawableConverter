FROM ubuntu:latest
LABEL info="https://github.com/a-student/SvgToVectorDrawableConverter"

RUN apt-get update
RUN apt-get install -y mono-devel inkscape

COPY release/ /release

ENTRYPOINT ["/usr/bin/mono", "/release/svg2vd.exe", "--no-update-check"]
