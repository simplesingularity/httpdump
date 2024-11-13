# httpdump
Acts as a HTTP server on Windows and dumps files that are POST'ed to it

## Purpose
The purpose of this is to quickly be able to get files out of Portainer, Docker, or any Linux distro using Curl.

Example:
You're using Windows and Putty to shell into your Ubuntu server that uses Docker. You shell into your container using:
```
sudo docker container ls
sudo docker exec -t -i mycontainer /bin/bash
```

You're inside your container shell and you need to backup a directory quickly so you run
```
sudo apt-get install curl
zip -r file.zip directory

```
Now you need to send it to your Windows machine so you start this project in debug mode and run this command (with your machine ip)
```
curl --data-binary "@./file.zip" http://192.168.1.3:80/
```

## Other alternatives
Other alternatives to accessing files inside running containers is using SSH / WINSCP. Basically just get into your container shell, install openssh-server and edit the config file to accept root logins in /etc/ssh/ and then use passwd to change root password. Then connect WinSCP to it. Usually you have to change the default port too since you usually have SSH running already inside the base machine that runs Portainer. Also known as a PITA.
I've also seen where people symlink folders into their containers, I haven't tried that but it sounds promising.