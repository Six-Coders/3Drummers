# Copyright 2023 The Magenta Authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""Configurations for transcription models."""

from __future__ import absolute_import
from __future__ import division
from __future__ import print_function

import collections

from  onsets_frames_transcription.tools.common import tf_utils  
from onsets_frames_transcription.tools.contrib import training as contrib_training
import onsets_frames_transcription.tools.audio_transform as audio_transform
import onsets_frames_transcription.tools.model_tpu as model_tpu

Config = collections.namedtuple('Config', ('model_fn', 'hparams'))




# Definiendo Hyperparámetros Generales

DEFAULT_HPARAMS = tf_utils.merge_hparams(
    audio_transform.DEFAULT_AUDIO_TRANSFORM_HPARAMS,
    contrib_training.HParams(
        eval_batch_size=1,
        predict_batch_size=1,
        shuffle_buffer_size=64,
        sample_rate=16000,
        spec_type='mel',
        spec_mel_htk=True,
        spec_log_amplitude=True,
        spec_hop_length=512,
        spec_n_bins=229,
        spec_fmin=30.0,  # A0
        cqt_bins_per_octave=36,
        truncated_length_secs=0.0,
        max_expected_train_example_len=0,
        onset_length=32,
        offset_length=32,
        onset_mode='length_ms',
        onset_delay=0,
        min_frame_occupancy_for_label=0.0,
        jitter_amount_ms=0,
        min_duration_ms=0,
        backward_shift_amount_ms=0,
        velocity_scale=80.0,
        velocity_bias=10.0,
        drum_data_map='',
        drum_prediction_map='',
        velocity_loss_weight=1.0,
        splice_n_examples=0,
        viterbi_decoding=False,
        viterbi_alpha=0.5))


#
DRUMS_CONFIGURATION = Config(
    model_fn=model_tpu.model_fn,
    hparams=tf_utils.merge_hparams(
        tf_utils.merge_hparams(DEFAULT_HPARAMS,
                               model_tpu.get_default_hparams()),
        contrib_training.HParams(
            drums_only=True,
            onset_length=0,
            velocity_scale=127.0,
            velocity_bias=0.0,
            batch_size=128,
            sample_rate=44100,
            spec_n_bins=250,
            hop_length=441,  # 10ms
            velocity_loss_weight=0.5,
            num_filters=[16, 16, 32],
            fc_size=256,
            onset_lstm_units=64,
            acoustic_rnn_dropout_keep_prob=0.50,
            drum_data_map='8-hit',
            learning_rate=0.0001,
            splice_n_examples=12,
            max_expected_train_example_len=100,
        )),
)
