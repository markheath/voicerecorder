'''**************************************************************************
'''*
'''* NAME: smbPitchShift.cpp
'''* VERSION: 1.2
'''* HOME URL: http://www.dspdimension.com
'''* KNOWN BUGS: none
'''*
'''* SYNOPSIS: Routine for doing pitch shifting while maintaining
'''* duration using the Short Time Fourier Transform.
'''*
'''* DESCRIPTION: The routine takes a pitchShift factor value which is between 0.5
'''* (one octave down) and 2. (one octave up). A value of exactly 1 does not change
'''* the pitch. numSampsToProcess tells the routine how many samples in indata[0...
'''* numSampsToProcess-1] should be pitch shifted and moved to outdata[0 ...
'''* numSampsToProcess-1]. The two buffers can be identical (ie. it can process the
'''* data in-place). fftFrameSize defines the FFT frame size used for the
'''* processing. Typical values are 1024, 2048 and 4096. It may be any value <=
'''* MAX_FRAME_LENGTH but it MUST be a power of 2. osamp is the STFT
'''* oversampling factor which also determines the overlap between adjacent STFT
'''* frames. It should at least be 4 for moderate scaling ratios. A value of 32 is
'''* recommended for best quality. sampleRate takes the sample rate for the signal 
'''* in unit Hz, ie. 44100 for 44.1 kHz audio. The data passed to the routine in 
'''* indata[] should be in the range [-1.0, 1.0), which is also the output range 
'''* for the data, make sure you scale the data accordingly (for 16bit signed integers
'''* you would have to divide (and multiply) by 32768). 
'''*
'''* COPYRIGHT 1999-2009 Stephan M. Bernsee <smb [AT] dspdimension [DOT] com>
'''*
'''* 						The Wide Open License (WOL)
'''*
'''* Permission to use, copy, modify, distribute and sell this software and its
'''* documentation for any purpose is hereby granted without fee, provided that
'''* the above copyright notice and this license appear in all source copies. 
'''* THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT EXPRESS OR IMPLIED WARRANTY OF
'''* ANY KIND. See http://www.dspguru.com/wol.htm for more information.
'''*
'''****************************************************************************

Namespace VoiceRecorder.Audio
    Friend Class SmbPitchShift
        Public Const M_PI_VAL = 3.1415926535897931
        Public Const MAX_FRAME_LENGTH = 8192

        Private Shared gInFIFO(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gOutFIFO(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gFFTworksp(2 * MAX_FRAME_LENGTH - 1) As Single
        Private Shared gLastPhase(MAX_FRAME_LENGTH \ 2) As Single
        Private Shared gSumPhase(MAX_FRAME_LENGTH \ 2) As Single
        Private Shared gOutputAccum(2 * MAX_FRAME_LENGTH - 1) As Single
        Private Shared gAnaFreq(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gAnaMagn(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gSynFreq(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gSynMagn(MAX_FRAME_LENGTH - 1) As Single
        Private Shared gRover As Integer = 0

        '''<summary>
        ''' Routine smbPitchShift(). See top of file for explanation
        ''' Purpose: doing pitch shifting while maintaining duration using the Short
        ''' Time Fourier Transform.
        ''' Author: (c)1999-2009 Stephan M. Bernsee &lt;smb [AT] dspdimension [DOT] com&gt;
        '''</summary>
        Public Shared Sub smbPitchShift(ByVal pitchShift As Single, ByVal numSampsToProcess As Integer,
                                        ByVal fftFrameSize As Integer, ByVal osamp As Integer,
                                        ByVal sampleRate As Single, ByVal indata() As Single, ByVal outdata() As Single)

            Dim magn, phase, tmp, window, real, imag As Double
            Dim freqPerBin, expct As Double
            Dim i, k, qpd, index, inFifoLatency, stepSize, fftFrameSize2 As Integer

            ' set up some handy variables 
            fftFrameSize2 = fftFrameSize \ 2
            stepSize = fftFrameSize \ osamp
            freqPerBin = sampleRate / CDbl(fftFrameSize)
            expct = 2.0 * M_PI_VAL * CDbl(stepSize) / CDbl(fftFrameSize)
            inFifoLatency = fftFrameSize - stepSize
            If gRover = 0 Then
                gRover = inFifoLatency
            End If

            ' main processing loop 
            For i = 0 To numSampsToProcess - 1

                ' As long as we have not yet collected enough data just read in 
                gInFIFO(gRover) = indata(i)
                outdata(i) = gOutFIFO(gRover - inFifoLatency)
                gRover += 1

                ' now we have enough data for processing 
                If gRover >= fftFrameSize Then
                    gRover = inFifoLatency

                    ' do windowing and re,im interleave 
                    For k = 0 To fftFrameSize - 1
                        window = -0.5 * Math.Cos(2.0 * M_PI_VAL * CDbl(k) / CDbl(fftFrameSize)) + 0.5
                        gFFTworksp(2 * k) = CSng(gInFIFO(k) * window)
                        gFFTworksp(2 * k + 1) = 0.0F
                    Next k


                    ' ***************** ANALYSIS ******************* 
                    ' do transform 
                    smbFft(gFFTworksp, fftFrameSize, -1)

                    ' this is the analysis step 
                    For k = 0 To fftFrameSize2

                        ' de-interlace FFT buffer 
                        real = gFFTworksp(2 * k)
                        imag = gFFTworksp(2 * k + 1)

                        ' compute magnitude and phase 
                        magn = 2.0 * Math.Sqrt(real * real + imag * imag)
                        phase = Math.Atan2(imag, real)

                        ' compute phase difference 
                        tmp = phase - gLastPhase(k)
                        gLastPhase(k) = CSng(phase)

                        ' subtract expected phase difference 
                        tmp -= CDbl(k) * expct

                        ' map delta phase into +/- Pi interval 
                        qpd = CInt(Fix(tmp / M_PI_VAL))
                        If qpd >= 0 Then
                            qpd += (qpd And 1)
                        Else
                            qpd -= (qpd And 1)
                        End If
                        tmp -= M_PI_VAL * CDbl(qpd)

                        ' get deviation from bin frequency from the +/- Pi interval 
                        tmp = osamp * tmp / (2.0 * M_PI_VAL)

                        ' compute the k-th partials' true frequency 
                        tmp = CDbl(k) * freqPerBin + tmp * freqPerBin

                        ' store magnitude and true frequency in analysis arrays 
                        gAnaMagn(k) = CSng(magn)
                        gAnaFreq(k) = CSng(tmp)

                    Next k

                    ' ***************** PROCESSING ******************* 
                    ' this does the actual pitch shifting 
                    Array.Clear(gSynMagn, 0, fftFrameSize)
                    Array.Clear(gSynFreq, 0, fftFrameSize)
                    For k = 0 To fftFrameSize2
                        index = CInt(Fix(k * pitchShift))
                        If index <= fftFrameSize2 Then
                            gSynMagn(index) += gAnaMagn(k)
                            gSynFreq(index) = gAnaFreq(k) * pitchShift
                        End If
                    Next k

                    ' ***************** SYNTHESIS ******************* 
                    ' this is the synthesis step 
                    For k = 0 To fftFrameSize2

                        ' get magnitude and true frequency from synthesis arrays 
                        magn = gSynMagn(k)
                        tmp = gSynFreq(k)

                        ' subtract bin mid frequency 
                        tmp -= CDbl(k) * freqPerBin

                        ' get bin deviation from freq deviation 
                        tmp /= freqPerBin

                        ' take osamp into account 
                        tmp = 2.0 * M_PI_VAL * tmp / osamp

                        ' add the overlap phase advance back in 
                        tmp += CDbl(k) * expct

                        ' accumulate delta phase to get bin phase 
                        gSumPhase(k) += CSng(tmp)
                        phase = gSumPhase(k)

                        ' get real and imag part and re-interleave 
                        gFFTworksp(2 * k) = CSng(magn * Math.Cos(phase))
                        gFFTworksp(2 * k + 1) = CSng(magn * Math.Sin(phase))
                    Next k

                    ' zero negative frequencies 
                    For k = fftFrameSize + 2 To 2 * fftFrameSize - 1
                        gFFTworksp(k) = 0.0F
                    Next k

                    ' do inverse transform 
                    smbFft(gFFTworksp, fftFrameSize, 1)

                    ' do windowing and add to output accumulator 
                    For k = 0 To fftFrameSize - 1
                        window = -0.5 * Math.Cos(2.0 * M_PI_VAL * CDbl(k) / CDbl(fftFrameSize)) + 0.5
                        gOutputAccum(k) += CSng(2.0 * window * gFFTworksp(2 * k) / (fftFrameSize2 * osamp))
                    Next k
                    For k = 0 To stepSize - 1
                        gOutFIFO(k) = gOutputAccum(k)
                    Next k

                    ' shift accumulator 
                    Dim destOffset = 0
                    Dim sourceOffset = stepSize
                    Array.Copy(gOutputAccum, sourceOffset, gOutputAccum, destOffset, fftFrameSize)
                    'memmove(gOutputAccum, gOutputAccum + stepSize, fftFrameSize * sizeof(float));

                    ' move input FIFO 
                    For k = 0 To inFifoLatency - 1
                        gInFIFO(k) = gInFIFO(k + stepSize)
                    Next k
                End If
            Next i
        End Sub

        ' -----------------------------------------------------------------------------------------------------------------


'         
'            FFT routine, (C)1996 S.M.Bernsee. Sign = -1 is FFT, 1 is iFFT (inverse)
'            Fills fftBuffer[0...2*fftFrameSize-1] with the Fourier transform of the
'            time domain data in fftBuffer[0...2*fftFrameSize-1]. The FFT array takes
'            and returns the cosine and sine parts in an interleaved manner, ie.
'            fftBuffer[0] = cosPart[0], fftBuffer[1] = sinPart[0], asf. fftFrameSize
'            must be a power of 2. It expects a complex input signal (see footnote 2),
'            ie. when working with 'common' audio signals our input signal has to be
'            passed as {in[0],0.,in[1],0.,in[2],0.,...} asf. In that case, the transform
'            of the frequencies of interest is in fftBuffer[0...fftFrameSize].
'        
        Public Shared Sub smbFft(ByVal fftBuffer() As Single, ByVal fftFrameSize As Integer, ByVal sign As Integer)
            Dim wr, wi, arg, temp As Single
            Dim p1, p2 As Integer ' MRH: were float*
            Dim tr, ti, ur, ui As Single
            Dim p1r, p1i, p2r, p2i As Integer ' MRH: were float*
            Dim i, bitm, j, le, le2, k As Integer
            Dim fftFrameSize2 = fftFrameSize * 2

            For i = 2 To fftFrameSize2 - 2 - 1 Step 2
                bitm = 2
                j = 0
                Do While bitm < fftFrameSize2
                    If (i And bitm) <> 0 Then
                        j += 1
                    End If
                    j <<= 1
                    bitm <<= 1
                Loop
                If i < j Then
                    p1 = i
                    p2 = j
                    temp = fftBuffer(p1)
                    fftBuffer(p1) = fftBuffer(p2)
                    p1 += 1
                    fftBuffer(p2) = temp
                    p2 += 1
                    temp = fftBuffer(p1)
                    fftBuffer(p1) = fftBuffer(p2)
                    fftBuffer(p2) = temp
                End If
            Next i
            Dim kmax = CInt(Fix(Math.Log(fftFrameSize) / Math.Log(2.0) + 0.5))
            k = 0
            le = 2
            Do While k < kmax
                le <<= 1
                le2 = le >> 1
                ur = 1.0f
                ui = 0.0f
                arg = CSng(M_PI_VAL / (le2 >> 1))
                wr = CSng(Math.Cos(arg))
                wi = CSng(sign * Math.Sin(arg))
                For j = 0 To le2 - 1 Step 2
                    p1r = j
                    p1i = p1r + 1
                    p2r = p1r + le2
                    p2i = p2r + 1
                    For i = j To fftFrameSize2 - 1 Step le
                        Dim p2rVal = fftBuffer(p2r)
                        Dim p2iVal = fftBuffer(p2i)
                        tr = p2rVal * ur - p2iVal * ui
                        ti = p2rVal * ui + p2iVal * ur
                        fftBuffer(p2r) = fftBuffer(p1r) - tr
                        fftBuffer(p2i) = fftBuffer(p1i) - ti
                        fftBuffer(p1r) += tr
                        fftBuffer(p1i) += ti
                        p1r += le
                        p1i += le
                        p2r += le
                        p2i += le
                    Next i
                    tr = ur * wr - ui * wi
                    ui = ur * wi + ui * wr
                    ur = tr
                Next j
                k += 1
            Loop
        End Sub

        ''' <summary>
        '''    12/12/02, smb
        '''
        '''    PLEASE NOTE:
        '''
        '''    There have been some reports on domain errors when the atan2() function was used
        '''    as in the above code. Usually, a domain error should not interrupt the program flow
        '''    (maybe except in Debug mode) but rather be handled "silently" and a global variable
        '''    should be set according to this error. However, on some occasions people ran into
        '''    this kind of scenario, so a replacement atan2() function is provided here.
        '''    If you are experiencing domain errors and your program stops, simply replace all
        '''    instances of atan2() with calls to the smbAtan2() function below.
        ''' </summary>
        Private Function smbAtan2(ByVal x As Double, ByVal y As Double) As Double
            Dim signx As Double
            If x > 0.0 Then
                signx = 1.0
            Else
                signx = -1.0
            End If

            If x = 0.0 Then
                Return 0.0
            End If
            If y = 0.0 Then
                Return signx * M_PI_VAL / 2.0
            End If

            Return Math.Atan2(x, y)
        End Function

    End Class
End Namespace
