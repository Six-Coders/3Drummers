"""Transcribe a recording of drums to audio."""

from __future__ import absolute_import
from __future__ import division
#from __future__ import print_function

import os
import warnings

from onsets_frames_transcription.tools import audio_label_data_utils
from onsets_frames_transcription.tools import configs
from onsets_frames_transcription.tools import data
from onsets_frames_transcription.tools import infer_util
from onsets_frames_transcription.tools import train_util

import six

from note_seq import midi_io
from note_seq.protobuf import music_pb2

import tensorflow.compat.v1 as tf


# Ignore Tensorflow and Python Warnings
warnings.filterwarnings("ignore")
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'
os.environ["CUDA_VISIBLE_DEVICES"] = "-1"
tf.compat.v1.logging.set_verbosity(tf.compat.v1.logging.ERROR)



MODEL_DIR = r"onsets_frames_transcription/model_checkpoint"
CHECKPOINT_PATH = r"onsets_frames_transcription/model_checkpoint/model.ckpt-569400"
HPARAMS = r""
LOAD_AUDIO_WITH_LIBROSA = False
SUFFIX = r""



def create_example(filepath, sample_rate, load_audio_with_librosa):
  
    wav_data = tf.gfile.Open(filepath, 'rb').read()
    example_list = list(
        audio_label_data_utils.process_record(
            wav_data=wav_data, 
            sample_rate=sample_rate,
            ns=music_pb2.NoteSequence(),
            example_id=six.ensure_text(filepath, 'utf-8'),
            min_length=0,
            max_length=-1,
            allow_empty_notesequence=True,
            load_audio_with_librosa=load_audio_with_librosa))
   
    assert len(example_list) == 1
    return example_list[0].SerializeToString()


def drum_transcription(filepaths_list, target_path, model_path):
    
    data_fn = data.provide_batch
    config = configs.DRUMS_CONFIGURATION
    hparams = config.hparams
    hparams.parse(HPARAMS)
    hparams.batch_size = 1
    hparams.truncated_length_secs = 0

    with tf.Graph().as_default():
        examples = tf.placeholder(tf.string, [None])

        dataset = data_fn(
            examples=examples,
            preprocess_examples=True,
            params=hparams,
            is_training=False,
            shuffle_examples=False,
            skip_n_initial_records=0)

        estimator = train_util.create_estimator(config.model_fn, 
                                                os.path.expanduser(MODEL_DIR), 
                                                hparams)

        iterator = tf.data.make_initializable_iterator(dataset)
        next_record = iterator.get_next()

        with tf.Session() as sess:
            sess.run([ tf.initializers.global_variables(), tf.initializers.local_variables()
            ])

            for filepath in filepaths_list:
                sess.run(iterator.initializer,
                         {examples: [
                             create_example(filepath, hparams.sample_rate,
                                            LOAD_AUDIO_WITH_LIBROSA)]})

                def transcription_data(params):
                    del params
                    return tf.data.Dataset.from_tensors(sess.run(next_record))
                
                input_fn = infer_util.labels_to_features_wrapper(
                    transcription_data)

                checkpoint_path = None
                if model_path:
                    checkpoint_path = os.path.expanduser(model_path)
                
                prediction_list = list(
                    estimator.predict(
                        input_fn,
                        checkpoint_path=checkpoint_path,
                        yield_single_examples=False))
                assert len(prediction_list) == 1

                

                sequence_prediction = music_pb2.NoteSequence.FromString(
                    prediction_list[0]['sequence_predictions'][0])

                filename_ext = os.path.basename(filepath)
                filename = os.path.splitext(filename_ext)[0]
                
                midi_filepaths =  target_path + "/" + filename + SUFFIX + '.midi'
                midi_io.sequence_proto_to_midi_file(
                    sequence_prediction, midi_filepaths)

def main(argv):
    filepath = argv[0]
    target_path = argv[1]
    model_path = argv[2]
    drum_transcription([filepath], target_path, model_path)

if __name__ == '__main__':
    tf.app.run(main)