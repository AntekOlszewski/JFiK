declare i32 @printf(i8*, ...)
declare i32 @__isoc99_scanf(i8*, ...)
@strp = constant [4 x i8] c"%d\0A\00"
@strs = constant [3 x i8] c"%d\00"
@strpd = constant [4 x i8] c"%f\0A\00"
@strf = constant [4 x i8] c"%f\0A\00"
@strd = constant [4 x i8] c"%lf\00"
@strl = constant [6 x i8] c"%lld\0A\00"
@strsd = constant [4 x i8] c"%lf\00"
define i32 @main() nounwind{
%x = alloca i32
store i32 4, i32* %x
%y = alloca i32
store i32 3, i32* %y
%1 = load i32, i32* %y
%2 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strp, i32 0, i32 0), i32 %1)
%3 = add i32 2, 2
%z = alloca i32
store i32 %3, i32* %z
%4 = load i32, i32* %z
%5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([4 x i8], [4 x i8]* @strp, i32 0, i32 0), i32 %4)
ret i32 0 }
