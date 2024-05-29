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
@str_fmt = private unnamed_addr constant [3 x i8] c"%s\00", align 1
define i32 @main() nounwind{
%x = alloca i1
store i1 0, i1* %x
%1 = load i1, i1* %x
%2 = getelementptr inbounds [5 x i8], [5 x i8]* @str_true, i32 0, i32 0
%3 = getelementptr inbounds [6 x i8], [6 x i8]* @str_false, i32 0, i32 0
%4 = select i1 %1, i8* %2, i8* %3
%5 = getelementptr inbounds [3 x i8], [3 x i8]* @str_fmt, i32 0, i32 0
call i32 (i8*, ...) @printf(i8* %5, i8* %4)
ret i32 0 }
