mov $0 16
mov $1 8
shl $0 $1
mov $2 64
shl $2 $1
mov $3 1
inc $0
nop
mov [$0] $2
inc $0
nop
mov [$0] $3
jump loop