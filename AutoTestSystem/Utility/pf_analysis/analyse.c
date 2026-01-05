#include <stdlib.h>
#include <stdio.h>
#include <strings.h>
#include <sndfile.h>
#include <math.h>
#include <complex.h>
#include <fftw3.h>
#include <assert.h>

#define MAX_PEAKS 10

static void calc_magnitude(double *out,double *mag, int N)
{
	double r2, i2;
	int i;

	for (i = 1; i < N / 2; i++) {
		r2 = out[i] * out[i];
		i2 = out[N - i] * out[N - i];

		mag[i] = sqrtf(r2 + i2);
	}
	mag[0] = 0.0;
}

static void process_wav_file(char *path,int start_time)
{
    sf_count_t count;

    SF_INFO sfinfo;
    SNDFILE *sndfile;
	double *in=NULL,*out=NULL,*mag=NULL;
	fftw_plan p;

    bzero(&sfinfo, sizeof(SF_INFO));
    sndfile = sf_open(path, SFM_READ, &sfinfo);
    if (sndfile == NULL)
    {
        perror("failed to open file");
        exit(EXIT_FAILURE);
    }

    // if (sfinfo.channels != 1)
    // {
        // perror("only monoral wav file is accepted.");
        // exit(EXIT_FAILURE);
    // }
	
	printf ("Channels: %d, Sample rate: %d\n", sfinfo.channels, sfinfo.samplerate);
	printf ("Frames: %d\n", sfinfo.frames);
	
	//check start time, start time should be not large than end time
	if(start_time<0 || (start_time+1)>(sfinfo.frames/sfinfo.samplerate)){
		perror("Not supportd start time\n");
		exit(EXIT_FAILURE);
	}
	
	//only get one-second frames to analyse
	int frames=sfinfo.samplerate;
	
	/* Allocate FFT buffers */
	in = (double *) fftw_malloc(sizeof(double) * frames);
	if (in == NULL)
		goto end;

	out = (double *) fftw_malloc(sizeof(double) * frames);
	if (out == NULL)
		goto end;

	mag = (double *) fftw_malloc(sizeof(double) * frames);
	if (mag == NULL)
		goto end;

    //check harmonics
	//---------------------

	printf("fftw_plan_r2r_1d\n");
	/* create FFT plan */
	p = fftw_plan_r2r_1d(frames, in, out, FFTW_R2HC, FFTW_MEASURE | FFTW_PRESERVE_INPUT);
	if (p == NULL)
		goto end;
	
	//move sart time
	printf ("Start time:%d, move to #%d frame\n",start_time,sfinfo.samplerate*start_time);
	sf_seek(sndfile,sfinfo.samplerate*start_time,SEEK_SET);

	//we only get one-second frames	
	if(sfinfo.channels==1){
		count = sf_readf_double(sndfile, in, frames);
	}
	else if(sfinfo.channels>1 && sfinfo.channels<=5){
		double *oneframe= (double *) fftw_malloc(sizeof(double) * sfinfo.channels);
		
		for(count=0;count<frames;count++){
			sf_readf_double(sndfile, oneframe, 1);
			in[count]=oneframe[0];//only get 1st channel
		}
	}
	else{
		printf("Not supportd channels:%d \n",sfinfo.channels);
		goto end;
	}
	printf("count: %d \n",count);
    	
	/* run FFT */
	printf("fftw_execute\n");
	fftw_execute(p);
	
	/* FFT out is real and imaginary numbers - calc magnitude for each */
	calc_magnitude(out, mag, frames);
	
	/* check data*/
	/*-------------------------------------------------------*/
	float hz = 1.0 / ((float) frames / (float) sfinfo.samplerate);
	float mean = 0.0, t, sigma = 0.0 ;
	int i, start = -1, end = -1, peak = 0, signals = 0;
	int err = 0, N = frames / 2;
	float sigma_k = 3.0f;

	/* calculate mean */
	for (i = 0; i < N; i++)
		mean += mag[i];
	mean /= (float) N;
	
	printf("Mean: %f\n",mean);

	/* calculate standard deviation */
	for (i = 0; i < N; i++) {
		t = mag[i] - mean;
		t *= t;
		sigma += t;
	}
	sigma /= (float) N;
	sigma = sqrtf(sigma);
	
	printf("Sigma: %f\n",sigma);

	/* clip any data less than k sigma + mean */
	for (i = 0; i < N; i++) {
		if (mag[i] > mean + sigma_k * sigma) {

			/* find peak start points */
			if (start == -1) {
				start = peak = end = i;
				signals++;
				printf("Start: %d\n",start);
			} else {
				if (mag[i] > mag[peak])
					peak = i;
				end = i;
			}
		} else if (start != -1) {
			/* Check if peak is as expected */
			
			float hz_peak = (float) (peak) * hz;
			printf("Detected peak at %2.2f Hz of %2.2f dB\n", hz_peak,10.0 * log10(mag[peak] / mean));
			//err |= check_peak(bat, a, end, peak, hz, mean,	p, channel, start);
			end = start = -1;
			if (signals == MAX_PEAKS)
				break;
		}
	}

	/*-------------------------------------------------------*/

	fftw_destroy_plan(p);
	
end:
	if(mag==NULL)
		fftw_free(mag);
	if(out==NULL)
		fftw_free(out);
	if(in==NULL)
		fftw_free(in);

    sf_close(sndfile);
}


int main(int argc, char** argv)
{
	
	if(argc!=3){
		printf("usage: %s <wav file> <start time>\n",argv[0]);
		return 0;
	}

    process_wav_file(argv[1],atoi(argv[2]));
    return 0;
}