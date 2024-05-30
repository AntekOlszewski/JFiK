declare i32 @printf(i8*, ...)
declare i32 @__isoc99_scanf(i8*, ...)
@strp = constant [4 x i8] c"%d\0A\00"
@strs = constant [3 x i8] c"%d\00"
@strpd = constant [4 x i8] c"%f\0A\00"
@strf = constant [4 x i8] c"%f\0A\00"
@strd = constant [4 x i8] c"%lf\00"
@strl = constant [6 x i8] c"%lld\0A\00"
@strsd = constant [4 x i8] c"%lf\00"
@str_bool = private unnamed_addr constant [3 x i8] c"%d\00", align 1
@str_true = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str_fmt = private unnamed_addr constant [4 x i8] c"%s
\00", align 1
define i32 @fibonacii(i32* %n) nounwind {
%result = alloca i32
store i32 0, i32* %result
%1 = load i32, i32* %n
%2 = icmp sge i32 1, %1
br i1 %2, label %true1, label %false1
true1:
%3 = load i32, i32* %n
store i32 %3, i32* %result
br label %end1
false1:
br label %end1
end1:
%4 = load i32, i32* %n
%5 = icmp slt i32 1, %4
br i1 %5, label %true2, label %false2
true2:
%6 = load i32, i32* %n
%7 = sub i32 %6, 1
%newN = alloca i32
store i32 %7, i32* %newN
%8 = load i32, i32* %n
%9 = sub i32 %8, 2
%newN2 = alloca i32
store i32 %9, i32* %newN2
%10 = call i32 @fibonacii(i32* %newN)
%fib1 = alloca i32
store i32 %10, i32* %fib1
%11 = call i32 @fibonacii(i32* %newN2)
%fib2 = alloca i32
store i32 %11, i32* %fib2
%12 = load i32, i32* %fib1
%13 = load i32, i32* %fib2
%14 = add i32 %12, %13
store i32 %14, i32* %result
br label %end2
false2:
br label %end2
end2:
%15 = load i32, i32* %result
ret i32 %15
}
define i32 @silnia(i32* %n) nounwind {
%res = alloca i32
store i32 1, i32* %res
br label %cond3
cond3:
%1 = load i32, i32* %n
%2 = icmp slt i32 1, %1
br i1 %2, label %true3, label %false3
true3:
%3 = load i32, i32* %res
%4 = load i32, i32* %n
%5 = mul i32 %3, %4
store i32 %5, i32* %res
%6 = load i32, i32* %n
%7 = sub i32 %6, 1
store i32 %7, i32* %n
br label %cond3
false3:
%8 = load i32, i32* %res
ret i32 %8
}
define i32 @main() nounwind{
%y = alloca i32
store i32 4, i32* %y
%1 = call i32 @silnia(i32* %y)
%z = alloca i32
store i32 %1, i32* %z
%fibo = alloca i32
store i32 4, i32* %fibo
%2 = call i32 @fibonacii(i32* %fibo)
%x = alloca i32
store i32 %2, i32* %x
%3 = load i32, i32* %z
%4 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strp, i32 0, i32 0), i32 %3)
%5 = load i32, i32* %x
%6 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strp, i32 0, i32 0), i32 %5)
ret i32 0 }
