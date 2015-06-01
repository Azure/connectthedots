#!/bin/sh -e

sudo cp $HOME/ctdgtwy/deploy_and_start_ctd_on_boot.sh /etc/init.d/
sudo update-rc.d deploy_and_start_ctd_on_boot.sh defaults
