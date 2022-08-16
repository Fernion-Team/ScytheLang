; ModuleID = 'myapp'
source_filename = "myapp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc"

%struct._iobuf = type { i8* }
%struct.__crt_locale_pointers = type { %struct.__crt_locale_data*, %struct.__crt_multibyte_data* }
%struct.__crt_locale_data = type opaque
%struct.__crt_multibyte_data = type opaque

$printf = comdat any

$_vfprintf_l = comdat any

$__local_stdio_printf_options = comdat any

$fprintf = comdat any

$"??_C@_0BI@CCJOGAJE@Couldn?8t?5perform?5input?4?$AA@" = comdat any

$"??_C@_02CBLLGHLK@r?$CL?$AA@" = comdat any

$"??_C@_0BI@MHENFJCC@File?5is?5not?5open?1valid?4?$AA@" = comdat any

@"??_C@_0BI@CCJOGAJE@Couldn?8t?5perform?5input?4?$AA@" = linkonce_odr dso_local unnamed_addr constant [24 x i8] c"Couldn't perform input.\00", comdat, align 1
@"??_C@_02CBLLGHLK@r?$CL?$AA@" = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"r+\00", comdat, align 1
@"??_C@_0BI@MHENFJCC@File?5is?5not?5open?1valid?4?$AA@" = linkonce_odr dso_local unnamed_addr constant [24 x i8] c"File is not open/valid.\00", comdat, align 1
@__local_stdio_printf_options._OptionsStorage = internal global i64 0, align 8
@strtmp = private unnamed_addr constant [14 x i8] c"Hello, world!\00", align 1

; Function Attrs: noinline nounwind optnone uwtable
define dso_local void @print(i8* noundef %0) #0 {
  %2 = alloca i8*, align 8
  store i8* %0, i8** %2, align 8
  %3 = load i8*, i8** %2, align 8
  %4 = call i32 (i8*, ...) @printf(i8* noundef %3)
  ret void
}

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local i32 @printf(i8* noundef %0, ...) #0 comdat {
  %2 = alloca i8*, align 8
  %3 = alloca i32, align 4
  %4 = alloca i8*, align 8
  store i8* %0, i8** %2, align 8
  %5 = bitcast i8** %4 to i8*
  call void @llvm.va_start(i8* %5)
  %6 = load i8*, i8** %4, align 8
  %7 = load i8*, i8** %2, align 8
  %8 = call %struct._iobuf* @__acrt_iob_func(i32 noundef 1)
  %9 = call i32 @_vfprintf_l(%struct._iobuf* noundef %8, i8* noundef %7, %struct.__crt_locale_pointers* noundef null, i8* noundef %6)
  store i32 %9, i32* %3, align 4
  %10 = bitcast i8** %4 to i8*
  call void @llvm.va_end(i8* %10)
  %11 = load i32, i32* %3, align 4
  ret i32 %11
}

; Function Attrs: nofree nosync nounwind willreturn
declare void @llvm.va_start(i8*) #1

declare dso_local %struct._iobuf* @__acrt_iob_func(i32 noundef) #2

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local i32 @_vfprintf_l(%struct._iobuf* noundef %0, i8* noundef %1, %struct.__crt_locale_pointers* noundef %2, i8* noundef %3) #0 comdat {
  %5 = alloca i8*, align 8
  %6 = alloca %struct.__crt_locale_pointers*, align 8
  %7 = alloca i8*, align 8
  %8 = alloca %struct._iobuf*, align 8
  store i8* %3, i8** %5, align 8
  store %struct.__crt_locale_pointers* %2, %struct.__crt_locale_pointers** %6, align 8
  store i8* %1, i8** %7, align 8
  store %struct._iobuf* %0, %struct._iobuf** %8, align 8
  %9 = load i8*, i8** %5, align 8
  %10 = load %struct.__crt_locale_pointers*, %struct.__crt_locale_pointers** %6, align 8
  %11 = load i8*, i8** %7, align 8
  %12 = load %struct._iobuf*, %struct._iobuf** %8, align 8
  %13 = call i64* @__local_stdio_printf_options()
  %14 = load i64, i64* %13, align 8
  %15 = call i32 @__stdio_common_vfprintf(i64 noundef %14, %struct._iobuf* noundef %12, i8* noundef %11, %struct.__crt_locale_pointers* noundef %10, i8* noundef %9)
  ret i32 %15
}

; Function Attrs: nofree nosync nounwind willreturn
declare void @llvm.va_end(i8*) #1

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local i64* @__local_stdio_printf_options() #0 comdat {
  ret i64* @__local_stdio_printf_options._OptionsStorage
}

declare dso_local i32 @__stdio_common_vfprintf(i64 noundef, %struct._iobuf* noundef, i8* noundef, %struct.__crt_locale_pointers* noundef, i8* noundef) #2

; Function Attrs: noinline nounwind optnone uwtable
define dso_local i8* @read() #0 {
  %1 = alloca i8*, align 8
  %2 = alloca [128 x i8], align 16
  %3 = getelementptr inbounds [128 x i8], [128 x i8]* %2, i64 0, i64 0
  %4 = call i8* @gets_s(i8* noundef %3, i64 noundef 127)
  %5 = icmp eq i8* %4, null
  br i1 %5, label %6, label %8

6:                                                ; preds = %0
  %7 = call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([24 x i8], [24 x i8]* @"??_C@_0BI@CCJOGAJE@Couldn?8t?5perform?5input?4?$AA@", i64 0, i64 0))
  store i8* null, i8** %1, align 8
  br label %10

8:                                                ; preds = %0
  %9 = getelementptr inbounds [128 x i8], [128 x i8]* %2, i64 0, i64 0
  store i8* %9, i8** %1, align 8
  br label %10

10:                                               ; preds = %8, %6
  %11 = load i8*, i8** %1, align 8
  ret i8* %11
}

declare dso_local i8* @gets_s(i8* noundef, i64 noundef) #2

; Function Attrs: noinline nounwind optnone uwtable
define dso_local i32 @FileCreate(i8* noundef %0) #0 {
  %2 = alloca i8*, align 8
  %3 = alloca %struct._iobuf*, align 8
  store i8* %0, i8** %2, align 8
  %4 = load i8*, i8** %2, align 8
  %5 = call %struct._iobuf* @fopen(i8* noundef %4, i8* noundef getelementptr inbounds ([3 x i8], [3 x i8]* @"??_C@_02CBLLGHLK@r?$CL?$AA@", i64 0, i64 0))
  store %struct._iobuf* %5, %struct._iobuf** %3, align 8
  %6 = ptrtoint %struct._iobuf** %3 to i32
  ret i32 %6
}

declare dso_local %struct._iobuf* @fopen(i8* noundef, i8* noundef) #2

; Function Attrs: noinline nounwind optnone uwtable
define dso_local void @FileWrite(i32 noundef %0, i8* noundef %1) #0 {
  %3 = alloca i8*, align 8
  %4 = alloca i32, align 4
  store i8* %1, i8** %3, align 8
  store i32 %0, i32* %4, align 4
  %5 = load i32, i32* %4, align 4
  %6 = zext i32 %5 to i64
  %7 = inttoptr i64 %6 to %struct._iobuf*
  %8 = icmp eq %struct._iobuf* %7, null
  br i1 %8, label %9, label %11

9:                                                ; preds = %2
  %10 = call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([24 x i8], [24 x i8]* @"??_C@_0BI@MHENFJCC@File?5is?5not?5open?1valid?4?$AA@", i64 0, i64 0))
  br label %11

11:                                               ; preds = %9, %2
  %12 = load i8*, i8** %3, align 8
  %13 = load i32, i32* %4, align 4
  %14 = zext i32 %13 to i64
  %15 = inttoptr i64 %14 to %struct._iobuf*
  %16 = call i32 (%struct._iobuf*, i8*, ...) @fprintf(%struct._iobuf* noundef %15, i8* noundef %12)
  ret void
}

; Function Attrs: noinline nounwind optnone uwtable
define linkonce_odr dso_local i32 @fprintf(%struct._iobuf* noundef %0, i8* noundef %1, ...) #0 comdat {
  %3 = alloca i8*, align 8
  %4 = alloca %struct._iobuf*, align 8
  %5 = alloca i32, align 4
  %6 = alloca i8*, align 8
  store i8* %1, i8** %3, align 8
  store %struct._iobuf* %0, %struct._iobuf** %4, align 8
  %7 = bitcast i8** %6 to i8*
  call void @llvm.va_start(i8* %7)
  %8 = load i8*, i8** %6, align 8
  %9 = load i8*, i8** %3, align 8
  %10 = load %struct._iobuf*, %struct._iobuf** %4, align 8
  %11 = call i32 @_vfprintf_l(%struct._iobuf* noundef %10, i8* noundef %9, %struct.__crt_locale_pointers* noundef null, i8* noundef %8)
  store i32 %11, i32* %5, align 4
  %12 = bitcast i8** %6 to i8*
  call void @llvm.va_end(i8* %12)
  %13 = load i32, i32* %5, align 4
  ret i32 %13
}

; Function Attrs: noinline nounwind optnone uwtable
define dso_local i8* @FileReadAll(i32 noundef %0) #0 {
  %2 = alloca i8*, align 8
  %3 = alloca i32, align 4
  %4 = alloca %struct._iobuf*, align 8
  %5 = alloca i32, align 4
  %6 = alloca i8, align 1
  %7 = alloca i8*, align 8
  %8 = alloca i32, align 4
  store i32 %0, i32* %3, align 4
  %9 = load i32, i32* %3, align 4
  %10 = zext i32 %9 to i64
  %11 = inttoptr i64 %10 to %struct._iobuf*
  %12 = icmp eq %struct._iobuf* %11, null
  br i1 %12, label %13, label %15

13:                                               ; preds = %1
  %14 = call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([24 x i8], [24 x i8]* @"??_C@_0BI@MHENFJCC@File?5is?5not?5open?1valid?4?$AA@", i64 0, i64 0))
  br label %15

15:                                               ; preds = %13, %1
  %16 = load i32, i32* %3, align 4
  %17 = zext i32 %16 to i64
  %18 = inttoptr i64 %17 to %struct._iobuf*
  store %struct._iobuf* %18, %struct._iobuf** %4, align 8
  %19 = load %struct._iobuf*, %struct._iobuf** %4, align 8
  %20 = call i32 @fseek(%struct._iobuf* noundef %19, i32 noundef 0, i32 noundef 2)
  %21 = load %struct._iobuf*, %struct._iobuf** %4, align 8
  %22 = call i32 @ftell(%struct._iobuf* noundef %21)
  store i32 %22, i32* %5, align 4
  %23 = load %struct._iobuf*, %struct._iobuf** %4, align 8
  %24 = call i32 @fseek(%struct._iobuf* noundef %23, i32 noundef 0, i32 noundef 0)
  %25 = load i32, i32* %5, align 4
  %26 = sext i32 %25 to i64
  %27 = mul i64 %26, 1
  %28 = call noalias i8* @malloc(i64 noundef %27)
  store i8* %28, i8** %7, align 8
  store i32 0, i32* %8, align 4
  br label %29

29:                                               ; preds = %40, %15
  %30 = load %struct._iobuf*, %struct._iobuf** %4, align 8
  %31 = call i32 @fgetc(%struct._iobuf* noundef %30)
  %32 = trunc i32 %31 to i8
  store i8 %32, i8* %6, align 1
  %33 = load i8, i8* %6, align 1
  %34 = load i8*, i8** %7, align 8
  %35 = load i32, i32* %8, align 4
  %36 = sext i32 %35 to i64
  %37 = getelementptr inbounds i8, i8* %34, i64 %36
  store i8 %33, i8* %37, align 1
  %38 = load i32, i32* %8, align 4
  %39 = add nsw i32 %38, 1
  store i32 %39, i32* %8, align 4
  br label %40

40:                                               ; preds = %29
  %41 = load i32, i32* %8, align 4
  %42 = load i32, i32* %5, align 4
  %43 = icmp slt i32 %41, %42
  br i1 %43, label %29, label %44, !llvm.loop !4

44:                                               ; preds = %40
  %45 = load i8*, i8** %2, align 8
  ret i8* %45
}

declare dso_local i32 @fseek(%struct._iobuf* noundef, i32 noundef, i32 noundef) #2

declare dso_local i32 @ftell(%struct._iobuf* noundef) #2

declare dso_local noalias i8* @malloc(i64 noundef) #2

declare dso_local i32 @fgetc(%struct._iobuf* noundef) #2

; Function Attrs: noinline nounwind optnone uwtable
define dso_local void @FileClose(i32 noundef %0) #0 {
  %2 = alloca i32, align 4
  store i32 %0, i32* %2, align 4
  %3 = load i32, i32* %2, align 4
  %4 = zext i32 %3 to i64
  %5 = inttoptr i64 %4 to %struct._iobuf*
  %6 = icmp eq %struct._iobuf* %5, null
  br i1 %6, label %7, label %9

7:                                                ; preds = %1
  %8 = call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([24 x i8], [24 x i8]* @"??_C@_0BI@MHENFJCC@File?5is?5not?5open?1valid?4?$AA@", i64 0, i64 0))
  br label %9

9:                                                ; preds = %7, %1
  %10 = load i32, i32* %2, align 4
  %11 = zext i32 %10 to i64
  %12 = inttoptr i64 %11 to %struct._iobuf*
  %13 = call i32 @fclose(%struct._iobuf* noundef %12)
  ret void
}

declare dso_local i32 @fclose(%struct._iobuf* noundef) #2

define i32 @main() {
entry:
  %CallTMP = call void @print(i8* getelementptr inbounds ([14 x i8], [14 x i8]* @strtmp, i64 0, i64 0))
  ret i32 1
}

attributes #0 = { noinline nounwind optnone uwtable "frame-pointer"="none" "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #1 = { nofree nosync nounwind willreturn }
attributes #2 = { "frame-pointer"="none" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }

!llvm.ident = !{!0}
!llvm.module.flags = !{!1, !2, !3}

!0 = !{!"clang version 14.0.6"}
!1 = !{i32 1, !"wchar_size", i32 2}
!2 = !{i32 7, !"PIC Level", i32 2}
!3 = !{i32 7, !"uwtable", i32 1}
!4 = distinct !{!4, !5}
!5 = !{!"llvm.loop.mustprogress"}
