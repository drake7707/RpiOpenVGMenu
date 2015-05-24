if [ $1 = "-t0" ]
then
    #video
    param=$2
    echo "Playing file: $param"

    if [ $3 = "--subtitles" ]
    then
        omxplayer -o hdmi "$param" --subtitles "$4"
    else
        omxplayer -o hdmi "$param"
    fi
elif [ $1 = "-t1" ]
then
    #music
    param=$2
    echo "Playing file: $param"
    omxplayer -o hdmi "$param"
elif [ $1 = "-t2" ]
then
   #youtube
    param=$(youtube-dl -g "$2")
    echo "Playing youtube: $param"
    omxplayer -o hdmi "$param"

elif [ $1 = "-t3" ]
then
   #livestream
   param=$2
   echo "Playing live stream: $param"
   livestreamer $param best -np 'omxplayer -o hdmi'
fi
