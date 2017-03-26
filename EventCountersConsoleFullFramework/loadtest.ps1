param([int] $iterations = 3000)

$url = "http://127.0.0.1:5000/Home/Index"
& loadtest -k -n $iterations -c 16 --rps 10 $url