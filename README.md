# hysite
Blog engine on .net

[![Build docker image, push to dockerhub, deploy on linode](https://github.com/ghyston/hysite/actions/workflows/deploy_to_linode.yml/badge.svg)](https://github.com/ghyston/hysite/actions/workflows/deploy_to_linode.yml)
-----

This is a small hobby project for learning .net development and deployment. Actual blog contains some posts, but they are not so interesting, as git log for this repository.

## main features:
* using git repository as host for blog posts
* posts are markdown files with custom header
* hosted on [linode](http://linode.com/) linux vm, running in docker
* actual site is here: [Hyston Blog](https://hyston.blog/)

## Logging

Production writes daily files under `LogsLocalPath`: `hysite-all-*.log` contains
Information and higher levels, and `hysite-warnings-*.log` contains Warning and
higher levels. Both streams also go to the console. Set
`LogRetention:InformationDays` (default 2) and `LogRetention:WarningAndAboveDays`
(default 60) to change their retention periods. Environment variables such as
`LogRetention__InformationDays` can override these settings.

Old files are removed when the corresponding stream next opens a file, so cleanup
can be delayed while the application is stopped or the stream is idle. Retention
applies to whole daily files, not individual log entries. Existing `hysite-*.log`
files from the previous logger are not managed by the new retention policies and
must be removed separately. Development logs all levels to the console only.
